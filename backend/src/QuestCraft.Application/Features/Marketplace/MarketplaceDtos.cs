namespace QuestCraft.Application.Features.Marketplace;

public record MarketplaceItemDto(
    int Id,
    string Name,
    string? Description,
    int ItemTypeId,
    string ItemType,
    int Price,
    string? ImageUrl,
    bool IsActive,
    bool IsOwned,
    bool IsFeatured = false,
    bool IsWishlisted = false);

public record PurchaseResultDto(
    int PurchaseId, int MarketplaceItemId, string ItemName, string ItemType, string? ImageUrl,
    int PricePaid, int RemainingCoins, bool AutoEquipped = false);

public record MyPurchaseDto(
    int Id, int MarketplaceItemId, string ItemName, int ItemTypeId, string ItemType, string? ImageUrl,
    int PricePaid, DateTime PurchasedAt, bool IsEquipped);
