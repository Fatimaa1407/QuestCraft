using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Friends;

public record RemoveFriendCommand(int FriendUserId) : ICommand<Unit>;

public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public RemoveFriendCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var relation = await _context.FriendRequests.FirstOrDefaultAsync(
            f => f.Status == FriendRequestStatus.Accepted
                && ((f.RequesterId == userId && f.AddresseeId == request.FriendUserId)
                    || (f.RequesterId == request.FriendUserId && f.AddresseeId == userId)),
            cancellationToken)
            ?? throw new NotFoundException(nameof(FriendRequest), request.FriendUserId);

        _context.FriendRequests.Remove(relation);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
