using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

// The reference "transaction + rollback" flow described in docs/ARCHITECTURE.md §15: TransactionBehavior
// already opened a DB transaction before this handler runs, so any exception thrown below (insufficient
// balance, missing item) unwinds every write made so far — nothing partial ever reaches the database.
public record PurchaseItemCommand(int ItemId) : ICommand<PurchaseResultDto>;

public class PurchaseItemCommandHandler : IRequestHandler<PurchaseItemCommand, PurchaseResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PurchaseItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PurchaseResultDto> Handle(PurchaseItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        var item = await _context.MarketplaceItems.Include(i => i.ItemType)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId && i.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.ItemId);

        var alreadyOwned = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.MarketplaceItemId == item.Id, cancellationToken);
        if (alreadyOwned)
        {
            throw new ConflictException("Bu məhsul artıq alınıb.");
        }

        if (profile.Coins < item.Price)
        {
            throw new BadRequestException("Kifayət qədər coin yoxdur.");
        }

        profile.Coins -= item.Price;

        var purchase = new Purchase
        {
            UserId = userId,
            MarketplaceItemId = item.Id,
            PricePaid = item.Price,
            PurchasedAt = DateTime.UtcNow,
        };
        _context.Purchases.Add(purchase);

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null)
        {
            stats.TotalCoinsSpent += item.Price;
        }

        _context.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = "Purchase",
            EntityName = nameof(MarketplaceItem),
            EntityId = item.Id,
            NewValues = $"{{\"pricePaid\":{item.Price}}}",
        });

        await _context.SaveChangesAsync(cancellationToken);

        return new PurchaseResultDto(
            purchase.Id, item.Id, LocalizationHelper.Pick(item.Name, item.NameEn, _currentUser.IsEnglish),
            item.ItemType.Name, item.ImageUrl, item.Price, profile.Coins);
    }
}
