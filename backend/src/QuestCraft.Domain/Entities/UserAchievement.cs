using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class UserAchievement : BaseEntity
{
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    // Showcased on the profile page — capped at 3 per user, enforced in PinAchievementCommand.
    public bool IsPinned { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = default!;
}
