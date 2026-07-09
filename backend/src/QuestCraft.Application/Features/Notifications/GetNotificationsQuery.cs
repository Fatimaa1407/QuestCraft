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

    public Task<PagedResult<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var query = _context.Notifications.Where(n => n.UserId == userId);
        if (request.UnreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var projected = query
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto(n.Id, n.Type.ToString(), n.Title, n.Message, n.IsRead, n.CreatedAt));

        return PagedResult<NotificationDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
