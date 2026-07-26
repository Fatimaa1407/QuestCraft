using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Wishlist : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int MarketplaceItemId { get; set; }
    public MarketplaceItem MarketplaceItem { get; set; } = default!;
}
