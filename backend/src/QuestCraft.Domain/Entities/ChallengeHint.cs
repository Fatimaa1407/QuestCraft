using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ChallengeHint : BaseEntity
{
    public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;
}
