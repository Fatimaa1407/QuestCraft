using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record RespondFriendRequestCommand(int RequestId, bool Accept) : ICommand<Unit>;

public class RespondFriendRequestCommandHandler : IRequestHandler<RespondFriendRequestCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public RespondFriendRequestCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<Unit> Handle(RespondFriendRequestCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var friendRequest = await _context.FriendRequests
            .Include(f => f.Addressee)
            .FirstOrDefaultAsync(f => f.Id == request.RequestId, cancellationToken)
            ?? throw new NotFoundException(nameof(FriendRequest), request.RequestId);

        if (friendRequest.AddresseeId != userId)
        {
            throw new ForbiddenException("Bu istəyə cavab vermək icazəniz yoxdur.");
        }

        if (friendRequest.Status != FriendRequestStatus.Pending)
        {
            throw new ConflictException("Bu istəyə artıq cavab verilib.");
        }

        friendRequest.Status = request.Accept ? FriendRequestStatus.Accepted : FriendRequestStatus.Declined;
        friendRequest.RespondedAt = DateTime.UtcNow;

        if (request.Accept)
        {
            _context.Notifications.Add(new Notification
            {
                UserId = friendRequest.RequesterId,
                Type = NotificationType.FriendRequestAccepted,
                Title = "Dostluq istəyi qəbul edildi",
                Message = $"{friendRequest.Addressee.Username} dostluq istəyinizi qəbul etdi.",
                TitleEn = "Friend request accepted",
                MessageEn = $"{friendRequest.Addressee.Username} accepted your friend request.",
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        if (request.Accept)
        {
            await _realtimeNotifier.NotifyNewNotification(friendRequest.RequesterId, cancellationToken);
        }

        return Unit.Value;
    }
}
