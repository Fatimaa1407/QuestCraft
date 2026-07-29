using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record BattleSubmissionResultDto(bool AllPassed, int PassedTestCases, int TotalTestCases, string? CompileErrorMessage, BattleDto Battle);

public record SubmitBattleSolutionCommand(int BattleId, string SourceCode) : ICommand<BattleSubmissionResultDto>;

public class SubmitBattleSolutionCommandValidator : AbstractValidator<SubmitBattleSolutionCommand>
{
    public SubmitBattleSolutionCommandValidator()
    {
        RuleFor(x => x.BattleId).GreaterThan(0);
        RuleFor(x => x.SourceCode).NotEmpty().WithMessage("Kod boş ola bilməz.")
            .MaximumLength(50_000).WithMessage("Kod 50.000 simvoldan uzun ola bilməz.");
    }
}

public class SubmitBattleSolutionCommandHandler : IRequestHandler<SubmitBattleSolutionCommand, BattleSubmissionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICodeExecutionEngine _codeExecutionEngine;
    private readonly IBattleHubNotifier _battleHubNotifier;
    private readonly IAchievementEvaluator _achievementEvaluator;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SubmitBattleSolutionCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, ICodeExecutionEngine codeExecutionEngine,
        IBattleHubNotifier battleHubNotifier, IAchievementEvaluator achievementEvaluator, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _codeExecutionEngine = codeExecutionEngine;
        _battleHubNotifier = battleHubNotifier;
        _achievementEvaluator = achievementEvaluator;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<BattleSubmissionResultDto> Handle(SubmitBattleSolutionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var battle = await _context.Battles
            .Include(b => b.Challenge).ThenInclude(c => c.TestCases)
            .Include(b => b.Challenge).ThenInclude(c => c.HiddenTestCases)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Statistics)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.BattleId);

        if (battle.Status != BattleStatus.InProgress)
        {
            throw new ConflictException("Bu döyüş hazırda aktiv deyil.");
        }

        var participant = battle.Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw new ForbiddenException("Siz bu döyüşün iştirakçısı deyilsiniz.");

        if (participant.HasFinished)
        {
            throw new ConflictException("Siz artıq bu döyüşü bitirmisiniz.");
        }

        var testCaseInputs = battle.Challenge.TestCases
            .OrderBy(t => t.OrderIndex)
            .Select(t => new TestCaseInput(t.Id, t.Input, t.ExpectedOutput, IsHidden: false))
            .Concat(battle.Challenge.HiddenTestCases
                .OrderBy(h => h.OrderIndex)
                .Select(h => new TestCaseInput(h.Id, h.Input, h.ExpectedOutput, IsHidden: true)))
            .ToList();

        var execution = await _codeExecutionEngine.ExecuteAsync(
            request.SourceCode, testCaseInputs, battle.Challenge.TimeLimitMs, battle.Challenge.MemoryLimitMb, cancellationToken);

        var passedCount = execution.TestResults.Count(r => r.Passed);
        var allPassed = passedCount == testCaseInputs.Count && testCaseInputs.Count > 0;

        participant.PassedTestCases = passedCount;
        participant.TotalTestCases = testCaseInputs.Count;
        participant.SubmittedCode = request.SourceCode;

        // The first participant to fully solve it wins outright — the battle ends immediately rather
        // than waiting for everyone else, matching a "race to solve" duel/room format. Anyone still
        // mid-attempt is ranked by however many test cases they'd passed at that moment.
        var battleJustEnded = false;
        if (allPassed)
        {
            participant.Rank = battle.Participants.Count(p => p.HasFinished) + 1;
            participant.HasFinished = true;
            participant.FinishedAt = DateTime.UtcNow;

            if (participant.Rank == 1)
            {
                battleJustEnded = true;
                battle.Status = BattleStatus.Finished;
                battle.EndedAt = DateTime.UtcNow;
                battle.Version++;
            }
        }

        // Claim the win — just Battle.Status/EndedAt/Version plus this participant's own Rank, nothing
        // else — so that if Battle.Version reveals someone else claimed it first, the only pending
        // change to discard is our own. Ranking everyone else and granting the reward touch other
        // participants and are deliberately deferred until AFTER this save confirms we actually won;
        // computing them speculatively here would mutate other participants' tracked entities based on
        // a stale "nobody else has finished yet" snapshot, corrupting them once the real winner's
        // already-committed state turns out to differ.
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException) when (battleJustEnded)
        {
            // Another participant's simultaneous full solve committed first and already ended the
            // battle between our read and our write (Battle.Version caught the conflict). Don't fight
            // over who owns Battle.Status/EndedAt — discard our pending Battle changes and re-rank
            // just this participant against the now-authoritative finisher count.
            _context.Entry(battle).State = EntityState.Unchanged;
            battleJustEnded = false;

            var finishedBeforeMe = await _context.BattleParticipants
                .CountAsync(p => p.BattleId == battle.Id && p.Id != participant.Id && p.HasFinished, cancellationToken);
            participant.Rank = finishedBeforeMe + 1;

            await _context.SaveChangesAsync(cancellationToken);
        }

        if (battleJustEnded)
        {
            // Only reachable once the claim above is confirmed to have actually won, so it's now safe
            // to rank everyone else, grant the reward, and log the similarity check without any
            // rollback concerns.
            BattleFinalizer.RankRemaining(battle, startingRank: 2);
            var winnerUserId = BattleFinalizer.GrantWinnerReward(_context, battle);
            BattleFinalizer.FlagSuspiciousSimilarity(_context, battle);
            await _context.SaveChangesAsync(cancellationToken);

            if (winnerUserId is not null)
            {
                await _achievementEvaluator.EvaluateAsync(winnerUserId.Value, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                await _realtimeNotifier.NotifyNewNotification(winnerUserId.Value, cancellationToken);
            }
        }

        var dto = BattleMapper.ToDto(battle, _currentUser.IsEnglish);
        await _battleHubNotifier.NotifyBattleUpdated(battle.Id, dto, cancellationToken);

        return new BattleSubmissionResultDto(allPassed, passedCount, testCaseInputs.Count, execution.CompileErrorMessage, dto);
    }
}
