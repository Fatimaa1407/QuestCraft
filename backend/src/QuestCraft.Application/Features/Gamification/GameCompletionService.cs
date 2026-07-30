using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record GameCompletionRewardDto(int MaxLevel, int BonusCoins, string? TitleText, string? BadgeImageUrl, string? BadgeName);

public interface IGameCompletionService
{
    /// <summary>Grants the one-time "finished all content" reward the first time a user's profile reaches
    /// the game's max level with that level's content fully complete. Idempotent — returns null on every
    /// call after the first (guarded by <see cref="UserProfile.GameCompletedAt"/>) and whenever the user
    /// hasn't actually reached 100% yet, so callers can invoke it unconditionally after every level
    /// recalculation without needing to track the transition themselves.</summary>
    Task<GameCompletionRewardDto?> TryGrantCompletionRewardAsync(int userId, CancellationToken cancellationToken);
}

public class GameCompletionService : IGameCompletionService
{
    // Reward-only cosmetics seeded once (see ApplicationDbContextSeeder.SeedGameCompletionRewardItemsAsync)
    // with IsActive = false so they never appear in the purchasable shop listing — the only way to own
    // them is finishing every level's content.
    public const string TitleItemName = "Quest Master";
    public const string BadgeItemName = "QuestCraft Completed";
    public const int CompletionBonusCoins = 500;

    private readonly IApplicationDbContext _context;
    private readonly IContentCompletionService _completionService;

    public GameCompletionService(IApplicationDbContext context, IContentCompletionService completionService)
    {
        _context = context;
        _completionService = completionService;
    }

    public async Task<GameCompletionRewardDto?> TryGrantCompletionRewardAsync(int userId, CancellationToken cancellationToken)
    {
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is null || profile.GameCompletedAt is not null)
        {
            return null;
        }

        var maxLevel = await _completionService.GetMaxAvailableLevelAsync(cancellationToken);
        if (profile.Level < maxLevel)
        {
            return null;
        }

        var completion = await _completionService.GetLevelCompletionAsync(userId, maxLevel, cancellationToken);
        if (!completion.IsComplete)
        {
            return null;
        }

        profile.Coins += CompletionBonusCoins;
        profile.GameCompletedAt = DateTime.UtcNow;

        var titleItem = await GrantRewardItemAsync(userId, TitleItemName, profile, cancellationToken);
        var badgeItem = await GrantRewardItemAsync(userId, BadgeItemName, profile, cancellationToken);

        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = NotificationType.GameCompleted,
            Title = "Bütün QuestCraft məzmunu tamamlandı!",
            Message = "Təbriklər! \"Quest Master\" rütbəsini və 500 bonus coin qazandın.",
            TitleEn = "All QuestCraft content completed!",
            MessageEn = "Congratulations! You've earned the \"Quest Master\" title and a 500-coin bonus.",
        });

        return new GameCompletionRewardDto(
            maxLevel,
            CompletionBonusCoins,
            titleItem?.Name,
            badgeItem?.ImageUrl,
            badgeItem?.Name);
    }

    // Grants ownership (a free Purchase row) of a reward-only item and auto-equips it, unless the user
    // somehow already owns it. Returns the item (for the response DTO) even when already owned/missing
    // from the seed, so a not-yet-seeded item just silently omits that part of the reward rather than
    // failing the whole grant.
    private async Task<MarketplaceItem?> GrantRewardItemAsync(int userId, string itemName, UserProfile profile, CancellationToken cancellationToken)
    {
        var item = await _context.MarketplaceItems.Include(i => i.ItemType).FirstOrDefaultAsync(i => i.Name == itemName, cancellationToken);
        if (item is null)
        {
            return null;
        }

        var alreadyOwned = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.MarketplaceItemId == item.Id, cancellationToken);
        if (!alreadyOwned)
        {
            _context.Purchases.Add(new Purchase { UserId = userId, MarketplaceItemId = item.Id, PricePaid = 0 });
            MarketplaceEquipHelper.Equip(profile, item);
        }

        return item;
    }
}
