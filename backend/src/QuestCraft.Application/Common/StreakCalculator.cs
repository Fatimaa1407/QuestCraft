using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Common;

public static class StreakCalculator
{
    // hasStreakFreeze = the user owns the "Streak Freeze" marketplace item — a one-time purchase
    // (same "buy once" rule as every other marketplace item, via PurchaseItemCommand's existing
    // ownership check) that grants permanent protection rather than a single-use consumable.
    public static void RecordActivity(Streak streak, DateOnly today, bool hasStreakFreeze = false)
    {
        if (streak.LastActivityDate == today)
        {
            return;
        }

        var continuesStreak = streak.LastActivityDate == today.AddDays(-1)
            || (hasStreakFreeze && streak.LastActivityDate == today.AddDays(-2));

        streak.CurrentStreak = continuesStreak ? streak.CurrentStreak + 1 : 1;

        streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
        streak.LastActivityDate = today;
    }
}
