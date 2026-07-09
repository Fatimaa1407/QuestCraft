using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ActivityLog : BaseEntity
{
    public DateOnly ActivityDate { get; set; }
    public int ActionCount { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
