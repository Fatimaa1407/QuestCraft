using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Chat;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

// Covers the SQL-side aggregation GetConversationsQuery was rewritten to use instead of pulling
// every message with every friend into memory (see the query's own comment) — the important thing
// to regression-test is that "last message" and "unread count" are still computed correctly per
// friend once that's grouped/aggregated in the database instead of in a LINQ-to-objects pass.
public class GetConversationsQueryHandlerTests
{
    [Fact]
    public async Task Handle_NoFriends_ReturnsEmpty()
    {
        var (db, userA, _) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new GetConversationsQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var result = await handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        Assert.Empty(result);
    }

    [Fact]
    public async Task Handle_FriendWithNoMessages_ReturnsConversationWithNullLastMessage()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var handler = new GetConversationsQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });

        var result = await handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        var conv = Assert.Single(result);
        Assert.Equal(userB.Id, conv.FriendUserId);
        Assert.Null(conv.LastMessage);
        Assert.Equal(0, conv.UnreadCount);
    }

    [Fact]
    public async Task Handle_MultipleMessages_ReturnsMostRecentAsLastMessage()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var now = DateTime.UtcNow;
        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "first", CreatedAt = now.AddMinutes(-10) });
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "second", CreatedAt = now.AddMinutes(-5) });
        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "third (latest)", CreatedAt = now });
        await db.SaveChangesAsync();

        var handler = new GetConversationsQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });
        var result = await handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        Assert.Equal("third (latest)", Assert.Single(result).LastMessage);
    }

    [Fact]
    public async Task Handle_UnreadCount_OnlyCountsMessagesFromFriendNotOwnSent()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "unread 1", IsRead = false });
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "unread 2", IsRead = false });
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "already read", IsRead = true });
        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "my own outgoing, never counts", IsRead = false });
        await db.SaveChangesAsync();

        var handler = new GetConversationsQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });
        var result = await handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        Assert.Equal(2, Assert.Single(result).UnreadCount);
    }

    [Fact]
    public async Task Handle_MultipleFriends_EachGetsOwnLastMessageAndCount()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var role = await db.Roles.FirstAsync();
        var userC = await BattleTestSupport.CreateUserAsync(db, "userC", role.Id);
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        db.FriendRequests.Add(new FriendRequest { RequesterId = userA.Id, AddresseeId = userC.Id, Status = FriendRequestStatus.Accepted });
        await db.SaveChangesAsync();

        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "from B" });
        db.ChatMessages.Add(new ChatMessage { SenderId = userC.Id, RecipientId = userA.Id, Content = "from C" });
        await db.SaveChangesAsync();

        var handler = new GetConversationsQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });
        var result = await handler.Handle(new GetConversationsQuery(), CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("from B", result.Single(c => c.FriendUserId == userB.Id).LastMessage);
        Assert.Equal("from C", result.Single(c => c.FriendUserId == userC.Id).LastMessage);
    }
}
