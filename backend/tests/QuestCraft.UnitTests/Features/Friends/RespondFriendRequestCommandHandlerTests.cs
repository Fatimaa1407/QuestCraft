using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Friends;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Friends;

public class RespondFriendRequestCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User Requester, User Addressee, FriendRequest Request)> SeedPendingAsync()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var request = new FriendRequest { RequesterId = userA.Id, AddresseeId = userB.Id, Status = FriendRequestStatus.Pending };
        db.FriendRequests.Add(request);
        await db.SaveChangesAsync();
        return (db, userA, userB, request);
    }

    [Fact]
    public async Task Handle_Accept_SetsAcceptedAndNotifiesRequester()
    {
        var (db, requester, addressee, request) = await SeedPendingAsync();
        var notifier = new FakeRealtimeNotifier();
        var handler = new RespondFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = addressee.Id }, notifier);

        await handler.Handle(new RespondFriendRequestCommand(request.Id, Accept: true), CancellationToken.None);

        var updated = await db.FriendRequests.FindAsync(request.Id);
        Assert.Equal(FriendRequestStatus.Accepted, updated!.Status);
        Assert.NotNull(updated.RespondedAt);
        Assert.True(await db.Notifications.AnyAsync(n => n.UserId == requester.Id && n.Type == NotificationType.FriendRequestAccepted));
        Assert.Contains(requester.Id, notifier.NotifiedUserIds);
    }

    [Fact]
    public async Task Handle_Decline_SetsDeclinedWithoutNotifying()
    {
        var (db, requester, addressee, request) = await SeedPendingAsync();
        var notifier = new FakeRealtimeNotifier();
        var handler = new RespondFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = addressee.Id }, notifier);

        await handler.Handle(new RespondFriendRequestCommand(request.Id, Accept: false), CancellationToken.None);

        var updated = await db.FriendRequests.FindAsync(request.Id);
        Assert.Equal(FriendRequestStatus.Declined, updated!.Status);
        Assert.Empty(notifier.NotifiedUserIds);
        _ = requester;
    }

    [Fact]
    public async Task Handle_NotTheAddressee_ThrowsForbidden()
    {
        var (db, requester, _, request) = await SeedPendingAsync();
        var handler = new RespondFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = requester.Id }, new FakeRealtimeNotifier());

        // The requester themselves tries to respond to their own outgoing request — only the
        // addressee may accept/decline it.
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new RespondFriendRequestCommand(request.Id, Accept: true), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyResponded_ThrowsConflict()
    {
        var (db, _, addressee, request) = await SeedPendingAsync();
        var handler = new RespondFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = addressee.Id }, new FakeRealtimeNotifier());
        await handler.Handle(new RespondFriendRequestCommand(request.Id, Accept: true), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new RespondFriendRequestCommand(request.Id, Accept: false), CancellationToken.None));
    }
}
