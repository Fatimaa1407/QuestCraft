using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class RateLimitLog : BaseEntity
{
    public string IpAddress { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
}
