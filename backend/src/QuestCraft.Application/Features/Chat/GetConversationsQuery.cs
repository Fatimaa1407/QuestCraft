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

        // Aggregated in SQL (grouped by conversation partner) rather than pulling every message ever
        // exchanged with every friend into memory just to pick the last one and count unread — that
        // approach doesn't scale with conversation history length.
        var lastByFriend = await _context.ChatMessages
            .Where(m => (m.SenderId == userId && friendIds.Contains(m.RecipientId))
                || (m.RecipientId == userId && friendIds.Contains(m.SenderId)))
            .GroupBy(m => m.SenderId == userId ? m.RecipientId : m.SenderId)
            .Select(g => new
            {
                FriendId = g.Key,
                Last = g.OrderByDescending(m => m.CreatedAt).Select(m => new { m.Content, m.CreatedAt }).First(),
            })
            .ToDictionaryAsync(x => x.FriendId, x => x.Last, cancellationToken);

        var unreadByFriend = await _context.ChatMessages
            .Where(m => m.RecipientId == userId && friendIds.Contains(m.SenderId) && !m.IsRead)
            .GroupBy(m => m.SenderId)
            .Select(g => new { FriendId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.FriendId, x => x.Count, cancellationToken);

        var conversations = friends.Select(f =>
        {
            lastByFriend.TryGetValue(f.UserId, out var last);
            unreadByFriend.TryGetValue(f.UserId, out var unread);

            return new ConversationDto(f.UserId, f.Username, f.AvatarUrl, last?.Content, last?.CreatedAt, unread, f.FrameImageUrl,
                LocalizationHelper.PickNullable(f.TitleName, f.TitleNameEn, isEnglish),
                f.BadgeImageUrl, LocalizationHelper.PickNullable(f.BadgeName, f.BadgeNameEn, isEnglish));
        })
        .OrderByDescending(c => c.LastMessageAt ?? DateTime.MinValue)
        .ToList();

        return conversations;
    }
}
