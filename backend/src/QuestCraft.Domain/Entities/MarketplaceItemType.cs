using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class MarketplaceItemType : BaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<MarketplaceItem> Items { get; set; } = new List<MarketplaceItem>();
}
