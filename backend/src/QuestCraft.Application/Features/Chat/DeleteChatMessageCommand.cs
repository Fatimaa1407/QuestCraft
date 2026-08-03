using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Chat;

public record DeleteChatMessageCommand(int MessageId) : ICommand<Unit>;

public class DeleteChatMessageCommandHandler : IRequestHandler<DeleteChatMessageCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public DeleteChatMessageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Unit> Handle(DeleteChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var message = await _context.ChatMessages.FirstOrDefaultAsync(m => m.Id == request.MessageId, cancellationToken)
            ?? throw new NotFoundException("ChatMessage", request.MessageId);

        // Only the sender can delete their own message — the recipient has no unilateral say over
        // what the other person sent, same as every mainstream chat app.
        if (message.SenderId != userId)
        {
            throw new ForbiddenException("Yalnız öz mesajlarınızı silə bilərsiniz.");
        }

        var recipientId = message.RecipientId;
        var messageId = message.Id;
        _context.ChatMessages.Remove(message);
        await _context.SaveChangesAsync(cancellationToken);

        await _realtimeNotifier.NotifyChatMessageDeleted(recipientId, messageId, userId, cancellationToken);

        return Unit.Value;
    }
}
