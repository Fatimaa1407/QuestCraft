using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Chat;

public record ClearConversationCommand(int WithUserId) : ICommand<Unit>;

public class ClearConversationCommandHandler : IRequestHandler<ClearConversationCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public ClearConversationCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Unit> Handle(ClearConversationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        // Not gated on an active friendship — clearing existing shared history with someone you've
        // since unfriended is still your call to make, unlike sending a brand-new message to them.
        var messages = await _context.ChatMessages
            .Where(m => (m.SenderId == userId && m.RecipientId == request.WithUserId)
                || (m.SenderId == request.WithUserId && m.RecipientId == userId))
            .ToListAsync(cancellationToken);

        if (messages.Count > 0)
        {
            _context.ChatMessages.RemoveRange(messages);
            await _context.SaveChangesAsync(cancellationToken);
        }

        await _realtimeNotifier.NotifyConversationCleared(request.WithUserId, userId, cancellationToken);

        return Unit.Value;
    }
}
