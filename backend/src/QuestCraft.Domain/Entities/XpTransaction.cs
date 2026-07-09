using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

// Every XP-awarding event (challenge solve, achievement, daily quest, future quiz) writes one row here.
// Period-based leaderboards (Daily/Weekly/Monthly) sum this log instead of any single reward source,
// so they stay accurate regardless of where the XP came from.
public class XpTransaction : BaseEntity
{
    public int Amount { get; set; }
    public string Source { get; set; } = default!;
    public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
