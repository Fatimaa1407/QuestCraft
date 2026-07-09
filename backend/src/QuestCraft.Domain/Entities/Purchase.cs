using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Purchase : BaseEntity
{
    public int PricePaid { get; set; }
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int MarketplaceItemId { get; set; }
    public MarketplaceItem MarketplaceItem { get; set; } = default!;
}
