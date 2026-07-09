using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Streak : BaseEntity
{
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public DateOnly? LastActivityDate { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
