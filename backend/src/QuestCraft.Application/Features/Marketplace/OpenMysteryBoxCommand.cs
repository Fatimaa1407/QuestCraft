using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

public record MysteryBoxResultDto(
    bool NothingLeft, int? ItemId, string? ItemName, string? ItemType, string? ImageUrl, string? Rarity,
    bool AutoEquipped, int PricePaid, int RemainingCoins);

public record OpenMysteryBoxCommand : ICommand<MysteryBoxResultDto>;

public class OpenMysteryBoxCommandHandler : IRequestHandler<OpenMysteryBoxCommand, MysteryBoxResultDto>
{
    public const int Cost = 100;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public OpenMysteryBoxCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MysteryBoxResultDto> Handle(OpenMysteryBoxCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        var ownedIds = await _context.Purchases.Where(p => p.UserId == userId).Select(p => p.MarketplaceItemId).ToListAsync(cancellationToken);

        var pool = await _context.MarketplaceItems
            .Include(i => i.ItemType)
            .Where(i => i.IsActive && i.ItemType.Name != "StreakFreeze" && !ownedIds.Contains(i.Id))
            .ToListAsync(cancellationToken);

        if (pool.Count == 0)
        {
            return new MysteryBoxResultDto(true, null, null, null, null, null, false, 0, profile.Coins);
        }

        if (profile.Coins < Cost)
        {
            throw new BadRequestException("Kifayət qədər coin yoxdur.");
        }

        profile.Coins -= Cost;

        var weighted = pool.Select(i => (Item: i, Weight: MarketplaceRarityHelper.GetWeight(MarketplaceRarityHelper.GetRarity(i.Price)))).ToList();
        var totalWeight = weighted.Sum(w => w.Weight);
        var roll = Random.Shared.Next(totalWeight);
        var cumulative = 0;
        var won = weighted[0].Item;
        foreach (var (item, weight) in weighted)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                won = item;
                break;
            }
        }

        _context.Purchases.Add(new Purchase
        {
            UserId = userId,
            MarketplaceItemId = won.Id,
            PricePaid = Cost,
            PurchasedAt = DateTime.UtcNow,
        });

        var autoEquipped = MarketplaceEquipHelper.IsEquipable(won.ItemType.Name);
        if (autoEquipped)
        {
            MarketplaceEquipHelper.Equip(profile, won);
        }

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null)
        {
            stats.TotalCoinsSpent += Cost;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "OpenMysteryBox",
            EntityName = nameof(MarketplaceItem),
            EntityId = won.Id,
            NewValues = $"{{\"pricePaid\":{Cost}}}",
        });

        await _context.SaveChangesAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        return new MysteryBoxResultDto(
            false, won.Id, LocalizationHelper.Pick(won.Name, won.NameEn, isEnglish), won.ItemType.Name, won.ImageUrl,
            MarketplaceRarityHelper.GetRarity(won.Price), autoEquipped, Cost, profile.Coins);
    }
}
