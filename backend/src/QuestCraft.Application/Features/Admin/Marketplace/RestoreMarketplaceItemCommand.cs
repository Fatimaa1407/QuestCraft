using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Marketplace;

public record RestoreMarketplaceItemCommand(int Id) : ICommand<MarketplaceItemDto>;

public class RestoreMarketplaceItemCommandHandler : IRequestHandler<RestoreMarketplaceItemCommand, MarketplaceItemDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreMarketplaceItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MarketplaceItemDto> Handle(RestoreMarketplaceItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _context.MarketplaceItems
            .IgnoreQueryFilters()
            .Include(i => i.ItemType)
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.Id);

        // Mirrors DeleteMarketplaceItemCommand's inverse — delete flips both IsDeleted and
        // IsActive off, so restore should bring the item back as active, not merely undeleted-
        // but-still-inactive (which would leave it invisible in the shop with no way to reactivate).
        item.IsDeleted = false;
        item.IsActive = true;
        await _context.SaveChangesAsync(cancellationToken);

        return new MarketplaceItemDto(item.Id, item.Name, item.Description, item.ItemTypeId, item.ItemType.Name, item.Price, item.ImageUrl, item.IsActive, false, item.IsFeatured);
    }
}
