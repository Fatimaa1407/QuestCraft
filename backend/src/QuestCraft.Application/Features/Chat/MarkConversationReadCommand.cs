using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Chat;

public record MarkConversationReadCommand(int WithUserId) : ICommand<Unit>;

public class MarkConversationReadCommandHandler : IRequestHandler<MarkConversationReadCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public MarkConversationReadCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(MarkConversationReadCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var unread = await _context.ChatMessages
            .Where(m => m.SenderId == request.WithUserId && m.RecipientId == userId && !m.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var message in unread)
        {
            message.IsRead = true;
        }

        if (unread.Count > 0)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
