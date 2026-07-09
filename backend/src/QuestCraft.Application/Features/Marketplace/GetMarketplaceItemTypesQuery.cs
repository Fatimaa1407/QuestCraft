using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Marketplace;

public record ItemTypeDto(int Id, string Name);

public record GetMarketplaceItemTypesQuery : IQuery<List<ItemTypeDto>>;

public class GetMarketplaceItemTypesQueryHandler : IRequestHandler<GetMarketplaceItemTypesQuery, List<ItemTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetMarketplaceItemTypesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<ItemTypeDto>> Handle(GetMarketplaceItemTypesQuery request, CancellationToken cancellationToken) =>
        _context.MarketplaceItemTypes.OrderBy(t => t.Name).Select(t => new ItemTypeDto(t.Id, t.Name)).ToListAsync(cancellationToken);
}
