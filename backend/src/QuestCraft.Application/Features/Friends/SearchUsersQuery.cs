using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record SearchUsersQuery(string Query) : IQuery<List<UserSearchResultDto>>;

public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQuery, List<UserSearchResultDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public SearchUsersQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<UserSearchResultDto>> Handle(SearchUsersQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        if (string.IsNullOrWhiteSpace(request.Query) || request.Query.Trim().Length < 2)
        {
            return [];
        }

        var term = request.Query.Trim();
        var candidates = await _context.UserProfiles
            .Include(p => p.User)
            .Where(p => p.UserId != userId && p.User.Username.Contains(term))
            .OrderBy(p => p.User.Username)
            .Take(20)
            .Select(p => new
            {
                p.UserId, p.User.Username,
                AvatarUrl = p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : p.AvatarUrl,
                p.Level,
                FrameImageUrl = p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null,
            })
            .ToListAsync(cancellationToken);

        var candidateIds = candidates.Select(c => c.UserId).ToList();
        var relations = await _context.FriendRequests
            .Where(f => (f.RequesterId == userId && candidateIds.Contains(f.AddresseeId))
                || (f.AddresseeId == userId && candidateIds.Contains(f.RequesterId)))
            .ToListAsync(cancellationToken);

        return candidates.Select(c =>
        {
            var relation = relations.FirstOrDefault(f => f.RequesterId == c.UserId || f.AddresseeId == c.UserId);
            var status = relation switch
            {
                { Status: FriendRequestStatus.Accepted } => "Friends",
                { Status: FriendRequestStatus.Pending } when relation.RequesterId == userId => "PendingSent",
                { Status: FriendRequestStatus.Pending } => "PendingReceived",
                _ => "None",
            };
            return new UserSearchResultDto(c.UserId, c.Username, c.AvatarUrl, c.Level, status, c.FrameImageUrl);
        }).ToList();
    }
}
