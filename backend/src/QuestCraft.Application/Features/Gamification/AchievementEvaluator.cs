using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

/// <summary>
/// Stages newly-unlocked achievements (UserAchievement + XP/Coin reward + Notification) on the context.
/// Does NOT call SaveChangesAsync — the calling command handler commits everything together.
/// </summary>
public interface IAchievementEvaluator
{
    Task<List<Achievement>> EvaluateAsync(int userId, CancellationToken cancellationToken);
}

public class AchievementEvaluator : IAchievementEvaluator
{
    private readonly IApplicationDbContext _context;

    public AchievementEvaluator(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Achievement>> EvaluateAsync(int userId, CancellationToken cancellationToken)
    {
        var unlockedIds = await _context.UserAchievements
            .Where(ua => ua.UserId == userId)
            .Select(ua => ua.AchievementId)
            .ToListAsync(cancellationToken);

        var candidates = await _context.Achievements
            .Where(a => a.IsActive && !unlockedIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return [];
        }

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        // A challenge counts as a "no-hint solve" if it was ever Accepted and no hint was unlocked for
        // it by this user (ChallengeHint.UnlockedAt has no time ordering requirement — unlocking a hint
        // after already solving it without one still disqualifies it, which matches player expectations).
        var noHintSolveCount = candidates.Any(a => a.ConditionType == AchievementConditionType.NoHintSolve)
            ? await _context.ChallengeSubmissions
                .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted)
                .Select(s => s.ChallengeId)
                .Distinct()
                .CountAsync(challengeId => !_context.ChallengeHints.Any(h => h.UserId == userId && h.ChallengeId == challengeId), cancellationToken)
            : 0;

        // "Fast solve" = accepted with a client-reported solve time under a minute. The timer is
        // client-controlled (like any quiz timer elsewhere in the app), so this is a soft signal used
        // only for this achievement, never for XP/coin rewards.
        const int SpeedSolveThresholdMs = 60_000;
        var speedSolveCount = candidates.Any(a => a.ConditionType == AchievementConditionType.SpeedSolve)
            ? await _context.ChallengeSubmissions
                .CountAsync(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted
                    && s.SolveTimeMs != null && s.SolveTimeMs <= SpeedSolveThresholdMs, cancellationToken)
            : 0;

        var unlocked = new List<Achievement>();

        foreach (var achievement in candidates)
        {
            var met = achievement.ConditionType switch
            {
                AchievementConditionType.SubmissionCount => stats?.TotalSubmissions >= achievement.ConditionValue,
                AchievementConditionType.AcceptedCount => stats?.TotalChallengesSolved >= achievement.ConditionValue,
                AchievementConditionType.XpTotal => profile?.Xp >= achievement.ConditionValue,
                AchievementConditionType.StreakDays => streak?.LongestStreak >= achievement.ConditionValue,
                AchievementConditionType.QuizzesCompleted => stats?.TotalQuizzesCompleted >= achievement.ConditionValue,
                AchievementConditionType.NoHintSolve => noHintSolveCount >= achievement.ConditionValue,
                AchievementConditionType.SpeedSolve => speedSolveCount >= achievement.ConditionValue,
                _ => false,
            };

            if (met != true)
            {
                continue;
            }

            _context.UserAchievements.Add(new UserAchievement { UserId = userId, AchievementId = achievement.Id });

            if (profile is not null)
            {
                profile.Xp += achievement.XpReward;
                profile.Coins += achievement.CoinReward;
                // Achievements grant XP/Coins only — Level is completion-based (see IContentCompletionService)
                // and only changes when a challenge/quiz from the current level is actually finished.
            }

            if (achievement.XpReward > 0)
            {
                _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = achievement.XpReward, Source = "Achievement" });
            }

            var nameEn = string.IsNullOrWhiteSpace(achievement.NameEn) ? achievement.Name : achievement.NameEn;
            _context.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = NotificationType.AchievementUnlock,
                Title = "Yeni nailiyyət!",
                Message = $"\"{achievement.Name}\" nailiyyətini əldə etdiniz.",
                TitleEn = "New achievement!",
                MessageEn = $"You unlocked \"{nameEn}\".",
            });

            unlocked.Add(achievement);
        }

        return unlocked;
    }
}
