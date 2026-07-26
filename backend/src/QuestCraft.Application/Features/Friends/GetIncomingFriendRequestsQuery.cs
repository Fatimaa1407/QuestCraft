using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
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

    public async Task<List<FriendRequestDto>> Handle(GetIncomingFriendRequestsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");
        var isEnglish = _currentUser.IsEnglish;

        var rows = await _context.FriendRequests
            .Include(f => f.Requester).ThenInclude(r => r.Profile)
            .Where(f => f.AddresseeId == userId && f.Status == FriendRequestStatus.Pending)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.Id, f.RequesterId, f.Requester.Username,
                AvatarUrl = f.Requester.Profile != null ? (f.Requester.Profile.EquippedAvatar != null ? f.Requester.Profile.EquippedAvatar.ImageUrl : f.Requester.Profile.AvatarUrl) : null,
                Level = f.Requester.Profile != null ? f.Requester.Profile.Level : 1,
                f.CreatedAt,
                FrameImageUrl = f.Requester.Profile != null && f.Requester.Profile.EquippedFrame != null ? f.Requester.Profile.EquippedFrame.ImageUrl : null,
                TitleName = f.Requester.Profile != null && f.Requester.Profile.EquippedTitle != null ? f.Requester.Profile.EquippedTitle.Name : null,
                TitleNameEn = f.Requester.Profile != null && f.Requester.Profile.EquippedTitle != null ? f.Requester.Profile.EquippedTitle.NameEn : null,
                BadgeImageUrl = f.Requester.Profile != null && f.Requester.Profile.EquippedBadge != null ? f.Requester.Profile.EquippedBadge.ImageUrl : null,
                BadgeName = f.Requester.Profile != null && f.Requester.Profile.EquippedBadge != null ? f.Requester.Profile.EquippedBadge.Name : null,
                BadgeNameEn = f.Requester.Profile != null && f.Requester.Profile.EquippedBadge != null ? f.Requester.Profile.EquippedBadge.NameEn : null,
            })
            .ToListAsync(cancellationToken);

        return rows.Select(f => new FriendRequestDto(
                f.Id, f.RequesterId, f.Username, f.AvatarUrl, f.Level, f.CreatedAt, f.FrameImageUrl,
                LocalizationHelper.PickNullable(f.TitleName, f.TitleNameEn, isEnglish),
                f.BadgeImageUrl, LocalizationHelper.PickNullable(f.BadgeName, f.BadgeNameEn, isEnglish)))
            .ToList();
    }
}
