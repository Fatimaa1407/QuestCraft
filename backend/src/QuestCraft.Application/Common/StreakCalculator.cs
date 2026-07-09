using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Common;

public static class StreakCalculator
{
    public static void RecordActivity(Streak streak, DateOnly today)
    {
        if (streak.LastActivityDate == today)
        {
            return;
        }

        streak.CurrentStreak = streak.LastActivityDate == today.AddDays(-1)
            ? streak.CurrentStreak + 1
            : 1;

        streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
        streak.LastActivityDate = today;
    }
}
