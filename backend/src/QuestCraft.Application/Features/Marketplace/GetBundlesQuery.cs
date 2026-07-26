using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Marketplace;

public record BundleItemDto(int MarketplaceItemId, string Name, string? ImageUrl, int Price, string ItemType, bool IsOwned);

public record MarketplaceBundleDto(
    int Id, string Name, string? Description, string? ImageUrl, int BundlePrice, int IndividualTotal,
    bool IsOwnedFully, int OwnedCount, List<BundleItemDto> Items);

public record GetBundlesQuery : IQuery<List<MarketplaceBundleDto>>;

public class GetBundlesQueryHandler : IRequestHandler<GetBundlesQuery, List<MarketplaceBundleDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetBundlesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MarketplaceBundleDto>> Handle(GetBundlesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        var ownedIds = userId is null
            ? []
            : await _context.Purchases.Where(p => p.UserId == userId).Select(p => p.MarketplaceItemId).ToListAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        var bundles = await _context.MarketplaceBundles
            .Where(b => b.IsActive)
            .Include(b => b.Items).ThenInclude(bi => bi.MarketplaceItem).ThenInclude(i => i.ItemType)
            .OrderBy(b => b.BundlePrice)
            .ToListAsync(cancellationToken);

        return bundles.Select(b =>
        {
            var items = b.Items.Select(bi => new BundleItemDto(
                bi.MarketplaceItemId,
                LocalizationHelper.Pick(bi.MarketplaceItem.Name, bi.MarketplaceItem.NameEn, isEnglish),
                bi.MarketplaceItem.ImageUrl, bi.MarketplaceItem.Price, bi.MarketplaceItem.ItemType.Name,
                ownedIds.Contains(bi.MarketplaceItemId)))
                .ToList();

            return new MarketplaceBundleDto(
                b.Id, LocalizationHelper.Pick(b.Name, b.NameEn, isEnglish),
                LocalizationHelper.PickNullable(b.Description, b.DescriptionEn, isEnglish),
                b.ImageUrl, b.BundlePrice, items.Sum(i => i.Price),
                items.Count > 0 && items.All(i => i.IsOwned), items.Count(i => i.IsOwned), items);
        }).ToList();
    }
}
