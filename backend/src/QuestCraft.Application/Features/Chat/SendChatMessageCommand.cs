using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Chat;

public record SendChatMessageCommand(int RecipientUserId, string Content) : ICommand<ChatMessageDto>;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.RecipientUserId).GreaterThan(0);
        RuleFor(x => x.Content).NotEmpty().WithMessage("Mesaj boş ola bilməz.")
            .MaximumLength(2000).WithMessage("Mesaj 2000 simvoldan uzun ola bilməz.");
    }
}

public class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SendChatMessageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<ChatMessageDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var areFriends = await _context.FriendRequests.AnyAsync(
            f => f.Status == FriendRequestStatus.Accepted
                && ((f.RequesterId == userId && f.AddresseeId == request.RecipientUserId)
                    || (f.RequesterId == request.RecipientUserId && f.AddresseeId == userId)),
            cancellationToken);

        if (!areFriends)
        {
            throw new ForbiddenException("Yalnız dostlarınıza mesaj göndərə bilərsiniz.");
        }

        var message = new ChatMessage
        {
            SenderId = userId,
            RecipientId = request.RecipientUserId,
            Content = request.Content.Trim(),
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ChatMessageDto(message.Id, message.SenderId, message.RecipientId, message.Content, message.CreatedAt, message.IsRead);

        await _realtimeNotifier.NotifyChatMessage(request.RecipientUserId, dto, cancellationToken);

        return dto;
    }
}
