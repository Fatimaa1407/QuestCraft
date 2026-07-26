using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

public record ToggleWishlistCommand(int ItemId) : ICommand<bool>;

// Returns the item's new wishlisted state (true = added, false = removed).
public class ToggleWishlistCommandHandler : IRequestHandler<ToggleWishlistCommand, bool>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ToggleWishlistCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<bool> Handle(ToggleWishlistCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var itemExists = await _context.MarketplaceItems.AnyAsync(i => i.Id == request.ItemId, cancellationToken);
        if (!itemExists)
        {
            throw new NotFoundException(nameof(MarketplaceItem), request.ItemId);
        }

        var existing = await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.MarketplaceItemId == request.ItemId, cancellationToken);

        if (existing is not null)
        {
            _context.Wishlists.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        _context.Wishlists.Add(new Wishlist { UserId = userId, MarketplaceItemId = request.ItemId });
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
