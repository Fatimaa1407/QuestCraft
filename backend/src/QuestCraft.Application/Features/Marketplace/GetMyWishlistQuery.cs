using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Marketplace;

public record GetMyWishlistQuery : IQuery<List<MarketplaceItemDto>>;

public class GetMyWishlistQueryHandler : IRequestHandler<GetMyWishlistQuery, List<MarketplaceItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyWishlistQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MarketplaceItemDto>> Handle(GetMyWishlistQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var ownedIds = await _context.Purchases.Where(p => p.UserId == userId).Select(p => p.MarketplaceItemId).ToListAsync(cancellationToken);

        var items = await _context.Wishlists
            .Where(w => w.UserId == userId)
            .Include(w => w.MarketplaceItem).ThenInclude(i => i.ItemType)
            .OrderByDescending(w => w.CreatedAt)
            .Select(w => w.MarketplaceItem)
            .ToListAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        return items.Select(i => new MarketplaceItemDto(
            i.Id,
            LocalizationHelper.Pick(i.Name, i.NameEn, isEnglish),
            LocalizationHelper.PickNullable(i.Description, i.DescriptionEn, isEnglish),
            i.ItemTypeId, i.ItemType.Name, i.Price, i.ImageUrl, i.IsActive, ownedIds.Contains(i.Id),
            i.IsFeatured, true))
            .ToList();
    }
}
