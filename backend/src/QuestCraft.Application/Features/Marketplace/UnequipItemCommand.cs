using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Marketplace;

public record UnequipItemCommand(int ItemId) : ICommand<Unit>;

public class UnequipItemCommandHandler : IRequestHandler<UnequipItemCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UnequipItemCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UnequipItemCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var item = await _context.MarketplaceItems.Include(i => i.ItemType)
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, cancellationToken)
            ?? throw new NotFoundException(nameof(MarketplaceItem), request.ItemId);

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        switch (item.ItemType.Name)
        {
            case "ProfileFrame" when profile.EquippedFrameId == item.Id:
                profile.EquippedFrameId = null;
                break;
            case "Title" when profile.EquippedTitleId == item.Id:
                profile.EquippedTitleId = null;
                break;
            case "Theme" when profile.EquippedThemeId == item.Id:
                profile.EquippedThemeId = null;
                break;
            case "Avatar" when profile.EquippedAvatarId == item.Id:
                profile.EquippedAvatarId = null;
                break;
            case "ProfileBanner" when profile.EquippedBannerId == item.Id:
                profile.EquippedBannerId = null;
                break;
            case "Badge" when profile.EquippedBadgeId == item.Id:
                profile.EquippedBadgeId = null;
                break;
            default:
                throw new BadRequestException("Bu məhsul taxılı deyil.");
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
