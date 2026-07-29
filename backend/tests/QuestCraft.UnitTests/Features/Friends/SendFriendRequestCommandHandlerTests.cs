using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Friends;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Friends;

public class SendFriendRequestCommandHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesRequestAndNotifies()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var notifier = new FakeRealtimeNotifier();
        var handler = new SendFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, notifier);

        await handler.Handle(new SendFriendRequestCommand(userB.Id), CancellationToken.None);

        Assert.True(await db.FriendRequests.AnyAsync(f => f.RequesterId == userA.Id && f.AddresseeId == userB.Id && f.Status == FriendRequestStatus.Pending));
        Assert.True(await db.Notifications.AnyAsync(n => n.UserId == userB.Id));
        Assert.Contains(userB.Id, notifier.NotifiedUserIds);
    }

    [Fact]
    public async Task Handle_SelfRequest_ThrowsConflict()
    {
        var (db, userA, _) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new SendFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SendFriendRequestCommand(userA.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyPending_ThrowsConflict()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new SendFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());
        await handler.Handle(new SendFriendRequestCommand(userB.Id), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SendFriendRequestCommand(userB.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyFriends_ThrowsConflict()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var handler = new SendFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SendFriendRequestCommand(userB.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_PreviouslyDeclined_ThrowsConflict()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.FriendRequests.Add(new FriendRequest { RequesterId = userA.Id, AddresseeId = userB.Id, Status = FriendRequestStatus.Declined });
        await db.SaveChangesAsync();
        var handler = new SendFriendRequestCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        // Matches the deliberate permanent-block behavior a declined request has today (see
        // SearchUsersQuery's "Declined" status, added specifically so the UI reflects this correctly).
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SendFriendRequestCommand(userB.Id), CancellationToken.None));
    }
}
