using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Marketplace;

namespace QuestCraft.Application.Features.Admin.Marketplace;

public record GetDeletedMarketplaceItemsQuery : IQuery<List<MarketplaceItemDto>>;

public class GetDeletedMarketplaceItemsQueryHandler : IRequestHandler<GetDeletedMarketplaceItemsQuery, List<MarketplaceItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedMarketplaceItemsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<MarketplaceItemDto>> Handle(GetDeletedMarketplaceItemsQuery request, CancellationToken cancellationToken) =>
        _context.MarketplaceItems
            .IgnoreQueryFilters()
            .Where(i => i.IsDeleted)
            .Include(i => i.ItemType)
            .OrderByDescending(i => i.UpdatedAt)
            .Select(i => new MarketplaceItemDto(
                i.Id, i.Name, i.Description, i.ItemTypeId, i.ItemType.Name, i.Price, i.ImageUrl, i.IsActive, false, i.IsFeatured))
            .ToListAsync(cancellationToken);
}
