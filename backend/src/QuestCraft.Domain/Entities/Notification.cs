using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class Notification : BaseEntity
{
    public NotificationType Type { get; set; }
    public string Title { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? TitleEn { get; set; }
    public string? MessageEn { get; set; }
    public bool IsRead { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
