using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

public record BundlePurchaseResultDto(int BundleId, string BundleName, int PricePaid, int RemainingCoins, List<string> GrantedItemNames);

public record PurchaseBundleCommand(int BundleId) : ICommand<BundlePurchaseResultDto>;

// Mirrors PurchaseItemCommand's transaction reasoning: TransactionBehavior already opened a DB
// transaction, so any exception below (already fully owned, insufficient balance) rolls back
// every write made so far.
public class PurchaseBundleCommandHandler : IRequestHandler<PurchaseBundleCommand, BundlePurchaseResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PurchaseBundleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<BundlePurchaseResultDto> Handle(PurchaseBundleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        var bundle = await _context.MarketplaceBundles
            .Include(b => b.Items).ThenInclude(bi => bi.MarketplaceItem).ThenInclude(i => i.ItemType)
            .FirstOrDefaultAsync(b => b.Id == request.BundleId && b.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceBundle), request.BundleId);

        if (bundle.Items.Count == 0)
        {
            throw new BadRequestException("Bu bundle boşdur.");
        }

        var ownedIds = await _context.Purchases
            .Where(p => p.UserId == userId && bundle.Items.Select(bi => bi.MarketplaceItemId).Contains(p.MarketplaceItemId))
            .Select(p => p.MarketplaceItemId)
            .ToListAsync(cancellationToken);

        if (bundle.Items.All(bi => ownedIds.Contains(bi.MarketplaceItemId)))
        {
            throw new ConflictException("Bu bundle-dəki bütün əşyalar artıq alınıb.");
        }

        if (profile.Coins < bundle.BundlePrice)
        {
            throw new BadRequestException("Kifayət qədər coin yoxdur.");
        }

        profile.Coins -= bundle.BundlePrice;

        var isEnglish = _currentUser.IsEnglish;
        var grantedNames = new List<string>();
        foreach (var bundleItem in bundle.Items)
        {
            if (ownedIds.Contains(bundleItem.MarketplaceItemId))
            {
                continue;
            }

            var item = bundleItem.MarketplaceItem;
            _context.Purchases.Add(new Purchase
            {
                UserId = userId,
                MarketplaceItemId = item.Id,
                PricePaid = 0,
                PurchasedAt = DateTime.UtcNow,
            });

            if (MarketplaceEquipHelper.IsEquipable(item.ItemType.Name))
            {
                MarketplaceEquipHelper.Equip(profile, item);
            }

            grantedNames.Add(LocalizationHelper.Pick(item.Name, item.NameEn, isEnglish));
        }

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null)
        {
            stats.TotalCoinsSpent += bundle.BundlePrice;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "PurchaseBundle",
            EntityName = nameof(MarketplaceBundle),
            EntityId = bundle.Id,
            NewValues = $"{{\"pricePaid\":{bundle.BundlePrice}}}",
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new BundlePurchaseResultDto(
            bundle.Id, LocalizationHelper.Pick(bundle.Name, bundle.NameEn, isEnglish),
            bundle.BundlePrice, profile.Coins, grantedNames);
    }
}
