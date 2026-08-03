using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Chat;

public record SendChatMessageCommand(int RecipientUserId, string? Content, string? ImageDataUrl) : ICommand<ChatMessageDto>;

public class SendChatMessageCommandValidator : AbstractValidator<SendChatMessageCommand>
{
    // ~4.5M base64 chars covers the ~3MB raw-file cap the frontend enforces before encoding —
    // kept as a server-side backstop since the frontend check is trivially bypassable.
    private const int MaxImageDataUrlLength = 4_500_000;

    public SendChatMessageCommandValidator()
    {
        RuleFor(x => x.RecipientUserId).GreaterThan(0);
        RuleFor(x => x.Content).MaximumLength(2000).WithMessage("Mesaj 2000 simvoldan uzun ola bilməz.");
        RuleFor(x => x.ImageDataUrl)
            .Must(url => url!.StartsWith("data:image/", StringComparison.Ordinal)).WithMessage("Şəkil formatı düzgün deyil.")
            .Must(url => url!.Length <= MaxImageDataUrlLength).WithMessage("Şəkil çox böyükdür (maksimum 3MB).")
            .When(x => x.ImageDataUrl != null);
        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.ImageDataUrl))
            .WithMessage("Mesaj boş ola bilməz.");
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
            Content = request.Content?.Trim() ?? string.Empty,
            ImageDataUrl = request.ImageDataUrl,
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new ChatMessageDto(message.Id, message.SenderId, message.RecipientId, message.Content, message.ImageDataUrl, message.CreatedAt, message.IsRead);

        await _realtimeNotifier.NotifyChatMessage(request.RecipientUserId, dto, cancellationToken);

        return dto;
    }
}
