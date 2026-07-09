using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class LeaderboardSnapshot : BaseEntity
{
    public LeaderboardPeriod Period { get; set; }
    public int Xp { get; set; }
    public int Rank { get; set; }
    public DateTime SnapshotDate { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
