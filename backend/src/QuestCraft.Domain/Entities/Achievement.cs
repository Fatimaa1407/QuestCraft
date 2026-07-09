using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class Achievement : BaseEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string? IconUrl { get; set; }
    public AchievementConditionType ConditionType { get; set; }
    public int ConditionValue { get; set; }
    public int XpReward { get; set; }
    public int CoinReward { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserAchievement> UnlockedBy { get; set; } = new List<UserAchievement>();
}
