using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public string Token { get; set; } = default!;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? CreatedByIp { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
