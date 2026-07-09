using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Submissions;

public record SubmitChallengeCommand(int ChallengeId, string SourceCode) : ICommand<SubmissionResultDto>;

public class SubmitChallengeCommandValidator : AbstractValidator<SubmitChallengeCommand>
{
    public SubmitChallengeCommandValidator()
    {
        RuleFor(x => x.ChallengeId).GreaterThan(0);
        RuleFor(x => x.SourceCode).NotEmpty().WithMessage("Kod boş ola bilməz.")
            .MaximumLength(50_000).WithMessage("Kod 50.000 simvoldan uzun ola bilməz.");
    }
}

public class SubmitChallengeCommandHandler : IRequestHandler<SubmitChallengeCommand, SubmissionResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICodeExecutionEngine _codeExecutionEngine;
    private readonly ICurrentUserService _currentUser;

    public SubmitChallengeCommandHandler(IApplicationDbContext context, ICodeExecutionEngine codeExecutionEngine, ICurrentUserService currentUser)
    {
        _context = context;
        _codeExecutionEngine = codeExecutionEngine;
        _currentUser = currentUser;
    }

    public async Task<SubmissionResultDto> Handle(SubmitChallengeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challenge = await _context.Challenges
            .Include(c => c.TestCases)
            .Include(c => c.HiddenTestCases)
            .FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.ChallengeId);

        if (!challenge.IsPublished && _currentUser.Role != "Admin")
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        var testCaseInputs = challenge.TestCases
            .OrderBy(t => t.OrderIndex)
            .Select(t => new TestCaseInput(t.Id, t.Input, t.ExpectedOutput, IsHidden: false))
            .Concat(challenge.HiddenTestCases
                .OrderBy(h => h.OrderIndex)
                .Select(h => new TestCaseInput(h.Id, h.Input, h.ExpectedOutput, IsHidden: true)))
            .ToList();

        var execution = await _codeExecutionEngine.ExecuteAsync(
            request.SourceCode, testCaseInputs, challenge.TimeLimitMs, challenge.MemoryLimitMb, cancellationToken);

        var alreadySolved = await _context.ChallengeSubmissions.AnyAsync(
            s => s.UserId == userId && s.ChallengeId == challenge.Id && s.Verdict == SubmissionVerdict.Accepted,
            cancellationToken);

        var submission = new ChallengeSubmission
        {
            UserId = userId,
            ChallengeId = challenge.Id,
            SourceCode = request.SourceCode,
            Verdict = execution.Verdict,
            ExecutionTimeMs = execution.ExecutionTimeMs,
            MemoryUsedKb = execution.MemoryUsedKb,
            SubmittedAt = DateTime.UtcNow,
        };

        submission.Results = execution.TestResults.Select(r => new SubmissionResult
        {
            TestCaseId = r.TestCaseId,
            IsHidden = r.IsHidden,
            Passed = r.Passed,
            ActualOutput = r.ActualOutput,
            ExecutionTimeMs = r.ExecutionTimeMs,
        }).ToList();

        _context.ChallengeSubmissions.Add(submission);

        var xpEarned = 0;
        var coinEarned = 0;
        var isFirstAcceptedSolve = execution.Verdict == SubmissionVerdict.Accepted && !alreadySolved;

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null)
        {
            stats.TotalSubmissions++;
            if (execution.Verdict == SubmissionVerdict.Accepted)
            {
                stats.AcceptedSubmissions++;
            }
        }

        if (isFirstAcceptedSolve)
        {
            xpEarned = challenge.XpReward;
            coinEarned = challenge.CoinReward;

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
            if (profile is not null)
            {
                profile.Xp += xpEarned;
                profile.Coins += coinEarned;
                profile.Level = GamificationCalculator.CalculateLevel(profile.Xp);
            }

            if (stats is not null)
            {
                stats.TotalChallengesSolved++;
                stats.TotalCoinsEarned += coinEarned;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        var testCaseResults = submission.Results.Select(r => r.IsHidden
            ? new SubmissionTestResultDto(true, r.Passed, null, null, null)
            : new SubmissionTestResultDto(false, r.Passed, GetInput(challenge, r.TestCaseId), GetExpectedOutput(challenge, r.TestCaseId), r.ActualOutput))
            .ToList();

        return new SubmissionResultDto(
            submission.Id,
            execution.Verdict.ToString(),
            testCaseResults.Count(r => r.Passed),
            testCaseResults.Count,
            execution.ExecutionTimeMs,
            execution.MemoryUsedKb,
            xpEarned,
            coinEarned,
            execution.CompileErrorMessage,
            testCaseResults);
    }

    private static string GetInput(Challenge challenge, int testCaseId) =>
        challenge.TestCases.First(t => t.Id == testCaseId).Input;

    private static string GetExpectedOutput(Challenge challenge, int testCaseId) =>
        challenge.TestCases.First(t => t.Id == testCaseId).ExpectedOutput;
}
