using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record GetFriendsQuery : IQuery<List<FriendDto>>;

public class GetFriendsQueryHandler : IRequestHandler<GetFriendsQuery, List<FriendDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetFriendsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<FriendDto>> Handle(GetFriendsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var friendIds = await _context.FriendRequests
            .Where(f => f.Status == FriendRequestStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(cancellationToken);

        return await _context.UserProfiles
            .Include(p => p.User)
            .Where(p => friendIds.Contains(p.UserId))
            .OrderBy(p => p.User.Username)
            .Select(p => new FriendDto(
                p.UserId, p.User.Username, p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : p.AvatarUrl, p.Level, p.Xp,
                p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null))
            .ToListAsync(cancellationToken);
    }
}
