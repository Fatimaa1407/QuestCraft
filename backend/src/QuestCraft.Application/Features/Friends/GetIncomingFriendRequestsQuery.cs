using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record GetIncomingFriendRequestsQuery : IQuery<List<FriendRequestDto>>;

public class GetIncomingFriendRequestsQueryHandler : IRequestHandler<GetIncomingFriendRequestsQuery, List<FriendRequestDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetIncomingFriendRequestsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public Task<List<FriendRequestDto>> Handle(GetIncomingFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        return _context.FriendRequests
            .Include(f => f.Requester).ThenInclude(r => r.Profile)
            .Where(f => f.AddresseeId == userId && f.Status == FriendRequestStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new FriendRequestDto(f.Id, f.RequesterId, f.Requester.Username, f.Requester.Profile != null ? f.Requester.Profile.AvatarUrl : null, f.Requester.Profile != null ? f.Requester.Profile.Level : 1, f.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
