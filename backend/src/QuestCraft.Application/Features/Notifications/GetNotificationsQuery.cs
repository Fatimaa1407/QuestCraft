using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Notifications;

public record GetNotificationsQuery(bool UnreadOnly, int Page, int PageSize) : IQuery<PagedResult<NotificationDto>>;

public class GetNotificationsQueryHandler : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetNotificationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private const int MaxRetainedPerUser = 30;

    public async Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        // Opportunistic retention: prune anything beyond the most recent N on every read, rather than
        // wiring cleanup into every notification-creation call site (achievements, daily quests, ...).
        var staleIds = await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(MaxRetainedPerUser)
            .Select(n => n.Id)
            .ToListAsync(cancellationToken);

        if (staleIds.Count > 0)
        {
            await _context.Notifications
                .Where(n => staleIds.Contains(n.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        var isEnglish = _currentUser.IsEnglish;

        var query = _context.Notifications.Where(n => n.UserId == userId);
        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var projected = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(
                n.Id,
                n.Type.ToString(),
                isEnglish && n.TitleEn != null && n.TitleEn != "" ? n.TitleEn : n.Title,
                isEnglish && n.MessageEn != null && n.MessageEn != "" ? n.MessageEn : n.Message,
                n.IsRead,
                n.CreatedAt));

        return await PagedResult<NotificationDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
