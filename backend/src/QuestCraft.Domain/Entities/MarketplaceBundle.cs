using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class MarketplaceBundle : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public string? ImageUrl { get; set; }
    public int BundlePrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<MarketplaceBundleItem> Items { get; set; } = new List<MarketplaceBundleItem>();
}

public class MarketplaceBundleItem : BaseEntity
{
    public int BundleId { get; set; }
    public MarketplaceBundle Bundle { get; set; } = default!;

    public int MarketplaceItemId { get; set; }
    public MarketplaceItem MarketplaceItem { get; set; } = default!;
}
