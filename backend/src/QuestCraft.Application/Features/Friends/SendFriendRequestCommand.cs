using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record SendFriendRequestCommand(int AddresseeUserId) : ICommand<Unit>;

public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.AddresseeUserId).GreaterThan(0);
    }
}

public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public SendFriendRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Unit> Handle(SendFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        if (userId == request.AddresseeUserId)
        {
            throw new ConflictException("Özünüzə dostluq istəyi göndərə bilməzsiniz.");
        }

        var addressee = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.AddresseeUserId, cancellationToken)
            ?? throw new NotFoundException(nameof(User), request.AddresseeUserId);

        var existing = await _context.FriendRequests.FirstOrDefaultAsync(
            f => (f.RequesterId == userId && f.AddresseeId == request.AddresseeUserId)
                || (f.RequesterId == request.AddresseeUserId && f.AddresseeId == userId),
            cancellationToken);

        if (existing is not null)
        {
            throw new ConflictException(existing.Status switch
            {
                FriendRequestStatus.Accepted => "Artıq dostsunuz.",
                FriendRequestStatus.Pending => "Dostluq istəyi artıq göndərilib.",
                _ => "Bu istifadəçiyə yenidən istək göndərə bilməzsiniz.",
            });
        }

        var requester = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);

        _context.FriendRequests.Add(new FriendRequest { RequesterId = userId, AddresseeId = request.AddresseeUserId });

        _context.Notifications.Add(new Notification
        {
            UserId = request.AddresseeUserId,
            Type = NotificationType.FriendRequest,
            Title = "Yeni dostluq istəyi",
            Message = $"{requester.Username} sizə dostluq istəyi göndərdi.",
            TitleEn = "New friend request",
            MessageEn = $"{requester.Username} sent you a friend request.",
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _realtimeNotifier.NotifyNewNotification(request.AddresseeUserId, cancellationToken);

        return Unit.Value;
    }
}
