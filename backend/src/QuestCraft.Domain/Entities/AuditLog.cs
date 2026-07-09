using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class AuditLog : BaseEntity
{
    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public int? EntityId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }

    public int? UserId { get; set; }
    public User? User { get; set; }
}
