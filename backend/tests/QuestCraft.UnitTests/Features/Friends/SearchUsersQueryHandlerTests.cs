using QuestCraft.Application.Features.Friends;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Friends;

public class SearchUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoRelation_ReturnsNone()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery(userB.Username[..5]), CancellationToken.None);

        Assert.Equal("None", Assert.Single(results).FriendStatus);
    }

    [Fact]
    public async Task Handle_Friends_ReturnsFriends()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery(userB.Username[..5]), CancellationToken.None);

        Assert.Equal("Friends", Assert.Single(results).FriendStatus);
    }

    [Fact]
    public async Task Handle_PendingSentByMe_ReturnsPendingSent()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.FriendRequests.Add(new FriendRequest { RequesterId = userA.Id, AddresseeId = userB.Id, Status = FriendRequestStatus.Pending });
        await db.SaveChangesAsync();
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery(userB.Username[..5]), CancellationToken.None);

        Assert.Equal("PendingSent", Assert.Single(results).FriendStatus);
    }

    [Fact]
    public async Task Handle_PendingReceivedByMe_ReturnsPendingReceived()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.FriendRequests.Add(new FriendRequest { RequesterId = userB.Id, AddresseeId = userA.Id, Status = FriendRequestStatus.Pending });
        await db.SaveChangesAsync();
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery(userB.Username[..5]), CancellationToken.None);

        Assert.Equal("PendingReceived", Assert.Single(results).FriendStatus);
    }

    [Fact]
    public async Task Handle_PreviouslyDeclined_ReturnsDeclinedNotNone()
    {
        // Regression test for the bug the audit found: SendFriendRequestCommand permanently blocks
        // re-sending after a decline, but this query used to fall through to "None" — showing a
        // working-looking "Add Friend" button that would always fail. It must surface "Declined".
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.FriendRequests.Add(new FriendRequest { RequesterId = userA.Id, AddresseeId = userB.Id, Status = FriendRequestStatus.Declined });
        await db.SaveChangesAsync();
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery(userB.Username[..5]), CancellationToken.None);

        Assert.Equal("Declined", Assert.Single(results).FriendStatus);
    }

    [Fact]
    public async Task Handle_ShortQuery_ReturnsEmpty()
    {
        var (db, userA, _) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new SearchUsersQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var results = await handler.Handle(new SearchUsersQuery("a"), CancellationToken.None);

        Assert.Empty(results);
    }
}
