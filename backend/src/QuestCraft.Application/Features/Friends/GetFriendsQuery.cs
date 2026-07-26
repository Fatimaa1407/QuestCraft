using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
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
        var isEnglish = _currentUser.IsEnglish;

        var friendIds = await _context.FriendRequests
            .Where(f => f.Status == FriendRequestStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(cancellationToken);

        var profiles = await _context.UserProfiles
            .Include(p => p.User)
            .Where(p => friendIds.Contains(p.UserId))
            .OrderBy(p => p.User.Username)
            .Select(p => new
            {
                p.UserId, p.User.Username,
                AvatarUrl = p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : p.AvatarUrl,
                p.Level, p.Xp,
                FrameImageUrl = p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null,
                TitleName = p.EquippedTitle != null ? p.EquippedTitle.Name : null,
                TitleNameEn = p.EquippedTitle != null ? p.EquippedTitle.NameEn : null,
                BadgeImageUrl = p.EquippedBadge != null ? p.EquippedBadge.ImageUrl : null,
                BadgeName = p.EquippedBadge != null ? p.EquippedBadge.Name : null,
                BadgeNameEn = p.EquippedBadge != null ? p.EquippedBadge.NameEn : null,
            })
            .ToListAsync(cancellationToken);

        return profiles.Select(p => new FriendDto(
                p.UserId, p.Username, p.AvatarUrl, p.Level, p.Xp, p.FrameImageUrl,
                LocalizationHelper.PickNullable(p.TitleName, p.TitleNameEn, isEnglish),
                p.BadgeImageUrl, LocalizationHelper.PickNullable(p.BadgeName, p.BadgeNameEn, isEnglish)))
            .ToList();
    }
}
