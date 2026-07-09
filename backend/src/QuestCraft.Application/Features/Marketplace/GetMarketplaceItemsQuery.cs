using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Marketplace;

public record GetMarketplaceItemsQuery(int? ItemTypeId) : IQuery<List<MarketplaceItemDto>>;

public class GetMarketplaceItemsQueryHandler : IRequestHandler<GetMarketplaceItemsQuery, List<MarketplaceItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMarketplaceItemsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<MarketplaceItemDto>> Handle(GetMarketplaceItemsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var ownedIds = userId is null
            ? []
            : await _context.Purchases.Where(p => p.UserId == userId).Select(p => p.MarketplaceItemId).ToListAsync(cancellationToken);

        var query = _context.MarketplaceItems.Include(i => i.ItemType).Where(i => i.IsActive).AsQueryable();
        if (request.ItemTypeId is not null)
        {
            query = query.Where(i => i.ItemTypeId == request.ItemTypeId);
        }

        var items = await query.OrderBy(i => i.Price).ToListAsync(cancellationToken);

        return items.Select(i => new MarketplaceItemDto(
            i.Id, i.Name, i.Description, i.ItemTypeId, i.ItemType.Name, i.Price, i.ImageUrl, i.IsActive, ownedIds.Contains(i.Id)))
            .ToList();
    }
}
