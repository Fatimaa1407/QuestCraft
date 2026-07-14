using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

/// <summary>
/// How many of a level's published challenges/quizzes a user has completed. A challenge counts once
/// it has an Accepted submission; a quiz counts once the user has any attempt (matches the existing
/// "isFirstAttempt" completion semantics used for daily-quest progress and the quiz list's checkmark).
/// </summary>
public record LevelCompletion(int Level, int ChallengesCompleted, int ChallengesTotal, int QuizzesCompleted, int QuizzesTotal)
{
    public int TotalCompleted => ChallengesCompleted + QuizzesCompleted;
    public int TotalItems => ChallengesTotal + QuizzesTotal;

    /// <summary>A level with no published content isn't "complete" — there's nothing to unlock the next level with.</summary>
    public bool IsComplete => TotalItems > 0 && TotalCompleted == TotalItems;
}

public interface IContentCompletionService
{
    Task<LevelCompletion> GetLevelCompletionAsync(int userId, int level, CancellationToken cancellationToken);

    /// <summary>The highest level a user has unlocked: 1 + however many consecutive levels (starting at 1)
    /// they've 100% completed. Stops at the first incomplete or empty (no content yet) level.</summary>
    Task<int> CalculateUnlockedLevelAsync(int userId, CancellationToken cancellationToken);
}

public class ContentCompletionService : IContentCompletionService
{
    private const int MaxLevelSearchDepth = 100;

    private readonly IApplicationDbContext _context;

    public ContentCompletionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LevelCompletion> GetLevelCompletionAsync(int userId, int level, CancellationToken cancellationToken)
    {
        var challengesTotal = await _context.Challenges
            .CountAsync(c => c.IsPublished && c.RequiredLevel == level, cancellationToken);
        var quizzesTotal = await _context.Quizzes
            .CountAsync(q => q.IsPublished && q.RequiredLevel == level, cancellationToken);

        var challengesCompleted = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted
                && s.Challenge.IsPublished && s.Challenge.RequiredLevel == level)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var quizzesCompleted = await _context.QuizAttempts
            .Where(a => a.UserId == userId && a.Quiz.IsPublished && a.Quiz.RequiredLevel == level)
            .Select(a => a.QuizId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new LevelCompletion(level, challengesCompleted, challengesTotal, quizzesCompleted, quizzesTotal);
    }

    public async Task<int> CalculateUnlockedLevelAsync(int userId, CancellationToken cancellationToken)
    {
        var level = 1;
        while (level < MaxLevelSearchDepth)
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
}
