using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Marketplace;

public record GetMyPurchasesQuery : IQuery<List<MyPurchaseDto>>;

public class GetMyPurchasesQueryHandler : IRequestHandler<GetMyPurchasesQuery, List<MyPurchaseDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyPurchasesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MyPurchaseDto>> Handle(GetMyPurchasesQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var equippedIds = new HashSet<int?>
        {
            profile?.EquippedFrameId, profile?.EquippedTitleId, profile?.EquippedThemeId,
            profile?.EquippedAvatarId, profile?.EquippedBannerId, profile?.EquippedBadgeId,
        };

        var purchases = await _context.Purchases
            .Include(p => p.MarketplaceItem).ThenInclude(i => i.ItemType)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PurchasedAt)
            .ToListAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        return purchases.Select(p => new MyPurchaseDto(
            p.Id, p.MarketplaceItemId,
            LocalizationHelper.Pick(p.MarketplaceItem.Name, p.MarketplaceItem.NameEn, isEnglish),
            p.MarketplaceItem.ItemTypeId, p.MarketplaceItem.ItemType.Name, p.MarketplaceItem.ImageUrl,
            p.PricePaid, p.PurchasedAt, equippedIds.Contains(p.MarketplaceItemId)))
            .ToList();
    }
}
