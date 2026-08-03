using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Chat;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

public class SendChatMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_Friends_SendsAndNotifies()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var notifier = new FakeRealtimeNotifier();
        var handler = new SendChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, notifier);

        var dto = await handler.Handle(new SendChatMessageCommand(userB.Id, "salam", null), CancellationToken.None);

        Assert.Equal("salam", dto.Content);
        Assert.True(await db.ChatMessages.AnyAsync(m => m.SenderId == userA.Id && m.RecipientId == userB.Id && m.Content == "salam"));
    }

    [Fact]
    public async Task Handle_ImageOnly_SendsWithEmptyContent()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var handler = new SendChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        var dto = await handler.Handle(new SendChatMessageCommand(userB.Id, null, "data:image/png;base64,iVBORw0KGgo="), CancellationToken.None);

        Assert.Equal(string.Empty, dto.Content);
        Assert.Equal("data:image/png;base64,iVBORw0KGgo=", dto.ImageDataUrl);
    }

    [Fact]
    public async Task Handle_NotFriends_ThrowsForbidden()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var handler = new SendChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        // The core social-graph guard the audit specifically checked for: a stranger (not on the
        // friend list) cannot message a user just by knowing their id.
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new SendChatMessageCommand(userB.Id, "salam", null), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnfriendedAfterward_NoLongerAllowed()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        await ChatFriendsTestSupport.MakeFriendsAsync(db, userA.Id, userB.Id);
        var relation = await db.FriendRequests.FirstAsync();
        db.FriendRequests.Remove(relation);
        await db.SaveChangesAsync();

        var handler = new SendChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new SendChatMessageCommand(userB.Id, "salam", null), CancellationToken.None));
    }
}
