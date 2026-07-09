using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Notifications;

public record MarkNotificationReadCommand(int Id) : ICommand<Unit>;

public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkNotificationReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.Id && n.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.Id);

        notification.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
