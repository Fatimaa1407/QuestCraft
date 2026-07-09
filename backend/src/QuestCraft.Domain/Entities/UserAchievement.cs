using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class UserAchievement : BaseEntity
{
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = default!;
}
