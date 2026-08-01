using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

/// <summary>
/// How many of a level's published challenges/quizzes a user has completed, plus their average score
/// across all of them. A challenge counts once it has an Accepted submission (worth 100% — a challenge
/// is pass/fail, so there's no partial credit to average in); a quiz counts once the user has any
/// attempt, and contributes their *best* attempt's score/totalQuestions ratio (so retaking a quiz to
/// improve a low score is exactly how a user is meant to raise their average — see IsComplete).
/// </summary>
public record LevelCompletion(
    int Level, int ChallengesCompleted, int ChallengesTotal, int QuizzesCompleted, int QuizzesTotal, double AverageScorePercent)
{
    // Below this, "completed" isn't good enough to unlock the next level — closes the loophole where
    // mashing random answers into every quiz (any attempt counts, regardless of score) advanced a user
    // exactly as fast as actually learning the material would have.
    public const double PassingAveragePercent = 70.0;

    public int TotalCompleted => ChallengesCompleted + QuizzesCompleted;
    public int TotalItems => ChallengesTotal + QuizzesTotal;

    /// <summary>A level with no published content isn't "complete" — there's nothing to unlock the next level with.
    /// Requires both full attendance (nothing skipped) and a passing average score.</summary>
    public bool IsComplete => TotalItems > 0 && TotalCompleted == TotalItems && AverageScorePercent >= PassingAveragePercent;
}

public interface IContentCompletionService
{
    Task<LevelCompletion> GetLevelCompletionAsync(int userId, int level, CancellationToken cancellationToken);

    /// <summary>The highest level a user has unlocked: 1 + however many consecutive levels (starting at 1)
    /// they've 100% completed, capped at <see cref="GetMaxAvailableLevelAsync"/> — a user can never be
    /// advanced past the highest level that actually has published content.</summary>
    Task<int> CalculateUnlockedLevelAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The highest RequiredLevel among any published challenge or quiz — the game's current content
    /// ceiling. Never hardcoded: as soon as an admin publishes higher-level content, this (and therefore
    /// everywhere that caps against it) picks it up automatically. Defaults to 1 if nothing is published yet.</summary>
    Task<int> GetMaxAvailableLevelAsync(CancellationToken cancellationToken);
}

public class ContentCompletionService : IContentCompletionService
{
    private readonly IApplicationDbContext _context;

    public ContentCompletionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LevelCompletion> GetLevelCompletionAsync(int userId, int level, CancellationToken cancellationToken)
    {
        // Battle Pool and Daily Puzzle challenges share the same RequiredLevel/IsPublished shape as
        // regular leveled content but are a separate pool entirely (excluded from GetChallengesQuery
        // too) — counting them here would inflate a level's total with questions that never actually
        // appear in that level's practice list.
        var challengesTotal = await _context.Challenges
            .CountAsync(c => c.IsPublished && c.RequiredLevel == level && !c.IsBattleOnly && !c.IsDailyPuzzle, cancellationToken);
        var quizzesTotal = await _context.Quizzes
            .CountAsync(q => q.IsPublished && q.RequiredLevel == level, cancellationToken);

        var challengesCompleted = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted
                && s.Challenge.IsPublished && s.Challenge.RequiredLevel == level
                && !s.Challenge.IsBattleOnly && !s.Challenge.IsDailyPuzzle)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var quizzesCompleted = await _context.QuizAttempts
            .Where(a => a.UserId == userId && a.Quiz.IsPublished && a.Quiz.RequiredLevel == level)
            .Select(a => a.QuizId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Per-attempt score ratios for this level's quizzes, pulled in as plain numbers so the
        // "best attempt per quiz" grouping below runs in memory rather than fighting EF's SQL
        // translation over a grouped-max-with-tiebreak query — the row count here is one user's
        // attempts at a single level, never large enough for that to matter.
        var quizAttemptRatios = await _context.QuizAttempts
            .Where(a => a.UserId == userId && a.Quiz.IsPublished && a.Quiz.RequiredLevel == level && a.TotalQuestions > 0)
            .Select(a => new { a.QuizId, Ratio = (double)a.Score / a.TotalQuestions })
            .ToListAsync(cancellationToken);
        var bestQuizRatioSum = quizAttemptRatios
            .GroupBy(a => a.QuizId)
            .Sum(g => g.Max(a => a.Ratio));

        var totalItems = challengesTotal + quizzesTotal;
        // Challenges are pass/fail (an Accepted submission already means every test case passed), so
        // each solved one simply contributes a full 100%; quizzes contribute their best ratio above.
        var averageScorePercent = totalItems > 0
            ? (challengesCompleted * 100.0 + bestQuizRatioSum * 100.0) / totalItems
            : 0.0;

        return new LevelCompletion(level, challengesCompleted, challengesTotal, quizzesCompleted, quizzesTotal, averageScorePercent);
    }

    public async Task<int> CalculateUnlockedLevelAsync(int userId, CancellationToken cancellationToken)
    {
        var maxAvailableLevel = await GetMaxAvailableLevelAsync(cancellationToken);

        var level = 1;
        while (level < maxAvailableLevel)
        {
            var completion = await GetLevelCompletionAsync(userId, level, cancellationToken);
            if (!completion.IsComplete)
            {
                break;
            }

            level++;
        }

        return level;
    }

    public async Task<int> GetMaxAvailableLevelAsync(CancellationToken cancellationToken)
    {
        var maxChallengeLevel = await _context.Challenges
            .Where(c => c.IsPublished && !c.IsBattleOnly && !c.IsDailyPuzzle)
            .Select(c => (int?)c.RequiredLevel)
            .MaxAsync(cancellationToken) ?? 1;

        var maxQuizLevel = await _context.Quizzes
            .Where(q => q.IsPublished)
            .Select(q => (int?)q.RequiredLevel)
            .MaxAsync(cancellationToken) ?? 1;

        return Math.Max(1, Math.Max(maxChallengeLevel, maxQuizLevel));
    }
}
