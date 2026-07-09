namespace QuestCraft.Application.Common;

/// <summary>
/// Shared XP → Level formula, reused everywhere XP changes (submissions, quizzes, achievements, daily quests).
/// RequiredXp(level) = 100 * level^1.5, treated as the cumulative XP threshold to reach that level.
/// </summary>
public static class GamificationCalculator
{
    public static int XpThresholdForLevel(int level) =>
        level <= 1 ? 0 : (int)Math.Round(100 * Math.Pow(level, 1.5));

    public static int CalculateLevel(int totalXp)
    {
        var level = 1;
        while (XpThresholdForLevel(level + 1) <= totalXp)
        {
            level++;
        }

        return level;
    }
}
