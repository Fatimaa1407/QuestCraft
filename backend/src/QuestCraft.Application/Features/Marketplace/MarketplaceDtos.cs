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
    bool IsOwned);

public record PurchaseResultDto(int PurchaseId, string ItemName, int PricePaid, int RemainingCoins);

public record MyPurchaseDto(
    int Id, int MarketplaceItemId, string ItemName, int ItemTypeId, string ItemType, string? ImageUrl,
    int PricePaid, DateTime PurchasedAt, bool IsEquipped);
