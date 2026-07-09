using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class MarketplaceItem : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int Price { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    public int ItemTypeId { get; set; }
    public MarketplaceItemType ItemType { get; set; } = default!;

    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
}
