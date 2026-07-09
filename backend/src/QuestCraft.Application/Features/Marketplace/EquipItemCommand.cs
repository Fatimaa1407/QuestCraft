using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

public record EquipItemCommand(int ItemId) : ICommand<Unit>;

public class EquipItemCommandHandler : IRequestHandler<EquipItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public EquipItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(EquipItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var owned = await _context.Purchases.AnyAsync(p => p.UserId == userId && p.MarketplaceItemId == request.ItemId, cancellationToken);
        if (!owned)
        {
            throw new BadRequestException("Bu məhsulu almamısınız.");
        }

        var item = await _context.MarketplaceItems.Include(i => i.ItemType)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.ItemId);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        switch (item.ItemType.Name)
        {
            case "ProfileFrame":
                profile.EquippedFrameId = item.Id;
                break;
            case "Title":
                profile.EquippedTitleId = item.Id;
                break;
            case "Theme":
                profile.EquippedThemeId = item.Id;
                break;
            default:
                throw new BadRequestException("Bu tip məhsul taxıla bilməz.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
