using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Chat;

public record GetConversationsQuery : IQuery<List<ConversationDto>>;

public class GetConversationsQueryHandler : IRequestHandler<GetConversationsQuery, List<ConversationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetConversationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<ConversationDto>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");
        var isEnglish = _currentUser.IsEnglish;

        var friendIds = await _context.FriendRequests
            .Where(f => f.Status == FriendRequestStatus.Accepted && (f.RequesterId == userId || f.AddresseeId == userId))
            .Select(f => f.RequesterId == userId ? f.AddresseeId : f.RequesterId)
            .ToListAsync(cancellationToken);

        if (friendIds.Count == 0)
        {
            return [];
        }

        var friends = await _context.UserProfiles
            .Include(p => p.User)
            .Where(p => friendIds.Contains(p.UserId))
            .Select(p => new
            {
                p.UserId, p.User.Username,
                AvatarUrl = p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : p.AvatarUrl,
                FrameImageUrl = p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null,
                TitleName = p.EquippedTitle != null ? p.EquippedTitle.Name : null,
                TitleNameEn = p.EquippedTitle != null ? p.EquippedTitle.NameEn : null,
                BadgeImageUrl = p.EquippedBadge != null ? p.EquippedBadge.ImageUrl : null,
                BadgeName = p.EquippedBadge != null ? p.EquippedBadge.Name : null,
                BadgeNameEn = p.EquippedBadge != null ? p.EquippedBadge.NameEn : null,
            })
            .ToListAsync(cancellationToken);

        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId && friendIds.Contains(m.RecipientId))
                || (m.RecipientId == userId && friendIds.Contains(m.SenderId)))
            .Select(m => new { m.SenderId, m.RecipientId, m.Content, m.CreatedAt, m.IsRead })
            .ToListAsync(cancellationToken);

        var conversations = friends.Select(f =>
        {
            var withFriend = messages.Where(m => m.SenderId == f.UserId || m.RecipientId == f.UserId).ToList();
            var last = withFriend.OrderByDescending(m => m.CreatedAt).FirstOrDefault();
            var unread = withFriend.Count(m => m.SenderId == f.UserId && !m.IsRead);

            return new ConversationDto(f.UserId, f.Username, f.AvatarUrl, last?.Content, last?.CreatedAt, unread, f.FrameImageUrl,
                LocalizationHelper.PickNullable(f.TitleName, f.TitleNameEn, isEnglish),
                f.BadgeImageUrl, LocalizationHelper.PickNullable(f.BadgeName, f.BadgeNameEn, isEnglish));
        })
        .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
        .ToList();

        return conversations;
    }
}
