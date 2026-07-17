using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Submissions;

public record SubmitChallengeCommand(int ChallengeId, string SourceCode, int? SolveTimeMs = null) : ICommand<SubmissionResultDto>;

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
    private readonly IDailyQuestService _dailyQuestService;
    private readonly IAchievementEvaluator _achievementEvaluator;
    private readonly IContentCompletionService _completionService;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SubmitChallengeCommandHandler(
        IApplicationDbContext context,
        ICodeExecutionEngine codeExecutionEngine,
        ICurrentUserService currentUser,
        IDailyQuestService dailyQuestService,
        IAchievementEvaluator achievementEvaluator,
        IContentCompletionService completionService,
        IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _codeExecutionEngine = codeExecutionEngine;
        _currentUser = currentUser;
        _dailyQuestService = dailyQuestService;
        _achievementEvaluator = achievementEvaluator;
        _completionService = completionService;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<SubmissionResultDto> Handle(SubmitChallengeCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challenge = await _context.Challenges
            .Include(c => c.TestCases)
            .Include(c => c.HiddenTestCases)
            .FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.ChallengeId);

        var isAdmin = _currentUser.Role == "Admin";

        if (!challenge.IsPublished && !isAdmin)
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        if (!isAdmin && challenge.RequiredLevel > 1)
        {
            var lockUserLevel = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => p.Level)
                .FirstOrDefaultAsync(cancellationToken);

            if (lockUserLevel < challenge.RequiredLevel)
            {
                throw new ForbiddenException($"Bu challenge üçün Level {challenge.RequiredLevel} lazımdır.");
            }
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
            SolveTimeMs = request.SolveTimeMs,
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

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var previousLevel = profile?.Level ?? 1;

        if (isFirstAcceptedSolve)
        {
            xpEarned = challenge.XpReward;
            coinEarned = challenge.CoinReward;

            if (profile is not null)
            {
                profile.Xp += xpEarned;
                profile.Coins += coinEarned;
            }

            if (stats is not null)
            {
                stats.TotalChallengesSolved++;
                stats.TotalCoinsEarned += coinEarned;
            }

            _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = xpEarned, Source = "Challenge" });
        }

        submission.XpEarned = xpEarned;
        submission.CoinEarned = coinEarned;

        if (execution.Verdict == SubmissionVerdict.Accepted)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
            if (streak is not null)
            {
                var hasStreakFreeze = await _context.Purchases
                    .AnyAsync(p => p.UserId == userId && p.MarketplaceItem.ItemType.Name == "StreakFreeze", cancellationToken);
                StreakCalculator.RecordActivity(streak, today, hasStreakFreeze);
            }

            var activityLog = await _context.ActivityLogs
                .FirstOrDefaultAsync(a => a.UserId == userId && a.ActivityDate == today, cancellationToken);
            if (activityLog is null)
            {
                _context.ActivityLogs.Add(new ActivityLog { UserId = userId, ActivityDate = today, ActionCount = 1 });
            }
            else
            {
                activityLog.ActionCount++;
            }

            if (isFirstAcceptedSolve)
            {
                await _dailyQuestService.UpdateProgressAsync(userId, DailyQuestTargetType.SolveChallenge, 1, cancellationToken);
            }
        }

        if (xpEarned > 0)
        {
            await _dailyQuestService.UpdateProgressAsync(userId, DailyQuestTargetType.EarnXp, xpEarned, cancellationToken);
        }

        if (isFirstAcceptedSolve && profile is not null)
        {
            // Flush first so the completion count below sees this submission.
            await _context.SaveChangesAsync(cancellationToken);
            profile.Level = await _completionService.CalculateUnlockedLevelAsync(userId, cancellationToken);
        }

        var newChallengesUnlocked = 0;
        var newQuizzesUnlocked = 0;
        if (profile is not null && profile.Level > previousLevel)
        {
            var newLevelContent = await _completionService.GetLevelCompletionAsync(userId, profile.Level, cancellationToken);
            newChallengesUnlocked = newLevelContent.ChallengesTotal;
            newQuizzesUnlocked = newLevelContent.QuizzesTotal;

            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = NotificationType.LevelUp,
                Title = "Yeni səviyyə!",
                Message = $"Səviyyə {profile.Level}-ə çatdınız!",
                TitleEn = "New level!",
                MessageEn = $"You reached Level {profile.Level}!",
            });
        }

        var newAchievements = await _achievementEvaluator.EvaluateAsync(userId, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        if ((profile is not null && profile.Level > previousLevel) || newAchievements.Count > 0)
        {
            await _realtimeNotifier.NotifyNewNotification(userId, cancellationToken);
        }

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
            testCaseResults,
            newAchievements.Select(a => a.Name).ToList(),
            profile?.Xp ?? 0,
            profile?.Coins ?? 0,
            profile?.Level ?? 1,
            previousLevel,
            newChallengesUnlocked,
            newQuizzesUnlocked);
    }

    private static string GetInput(Challenge challenge, int testCaseId) =>
        challenge.TestCases.First(t => t.Id == testCaseId).Input;

    private static string GetExpectedOutput(Challenge challenge, int testCaseId) =>
        challenge.TestCases.First(t => t.Id == testCaseId).ExpectedOutput;
}
