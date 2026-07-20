using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
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

    public SubmitBattleSolutionCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, ICodeExecutionEngine codeExecutionEngine, IBattleHubNotifier battleHubNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _codeExecutionEngine = codeExecutionEngine;
        _battleHubNotifier = battleHubNotifier;
    }

    public async Task<BattleSubmissionResultDto> Handle(SubmitBattleSolutionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var battle = await _context.Battles
            .Include(b => b.Challenge).ThenInclude(c => c.TestCases)
            .Include(b => b.Challenge).ThenInclude(c => c.HiddenTestCases)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
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

        if (allPassed)
        {
            // Count finishers BEFORE marking this one — participant is already one of the objects
            // inside battle.Participants, so flipping HasFinished first would count itself twice.
            participant.Rank = battle.Participants.Count(p => p.HasFinished) + 1;
            participant.HasFinished = true;
            participant.FinishedAt = DateTime.UtcNow;
        }

        // The first participant to fully solve it wins outright — the battle ends immediately rather
        // than waiting for everyone else, matching a "race to solve" duel/room format. Anyone still
        // mid-attempt is ranked by however many test cases they'd passed at that moment.
        var battleJustEnded = allPassed && participant.Rank == 1;
        if (battleJustEnded)
        {
            battle.Status = BattleStatus.Finished;
            battle.EndedAt = DateTime.UtcNow;

            var remaining = battle.Participants
                .Where(p => !p.HasFinished)
                .OrderByDescending(p => p.PassedTestCases)
                .ThenBy(p => p.Id)
                .ToList();

            var nextRank = 2;
            foreach (var p in remaining)
            {
                p.Rank = nextRank++;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var dto = BattleMapper.ToDto(battle);
        await _battleHubNotifier.NotifyBattleUpdated(battle.Id, dto, cancellationToken);

        return new BattleSubmissionResultDto(allPassed, passedCount, testCaseInputs.Count, execution.CompileErrorMessage, dto);
    }
}
