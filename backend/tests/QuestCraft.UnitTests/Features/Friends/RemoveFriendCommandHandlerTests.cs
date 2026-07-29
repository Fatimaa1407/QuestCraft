using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Friends;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Friends;

public class RemoveFriendCommandHandlerTests
{
    [Fact]
    public async Task Handle_ExistingFriendship_RemovesIt()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var handler = new RemoveFriendCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        await handler.Handle(new RemoveFriendCommand(userB.Id), CancellationToken.None);

        Assert.False(await db.FriendRequests.AnyAsync(f => f.RequesterId == userA.Id && f.AddresseeId == userB.Id));
    }

    [Fact]
    public async Task Handle_RemovableFromEitherSide_WorksForAddresseeToo()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        // userA was the original requester; userB (the addressee) should still be able to unfriend.
        var handler = new RemoveFriendCommandHandler(db, new FakeCurrentUserService { UserId = userB.Id });

        await handler.Handle(new RemoveFriendCommand(userA.Id), CancellationToken.None);

        Assert.False(await db.FriendRequests.AnyAsync());
    }

    [Fact]
    public async Task Handle_NotFriends_ThrowsNotFound()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new RemoveFriendCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new RemoveFriendCommand(userB.Id), CancellationToken.None));
    }
}
