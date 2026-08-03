using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Chat;
using QuestCraft.Domain.Entities;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

public class ClearConversationCommandHandlerTests
{
    [Fact]
    public async Task Handle_RemovesAllMessagesBetweenTheTwoUsers()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.ChatMessages.AddRange(
            new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "salam" },
            new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "necesen" });
        await db.SaveChangesAsync();

        var handler = new ClearConversationCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());
        await handler.Handle(new ClearConversationCommand(userB.Id), CancellationToken.None);

        Assert.Equal(0, await db.ChatMessages.CountAsync());
    }

    [Fact]
    public async Task Handle_DoesNotTouchMessagesWithOtherUsers()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var role = await db.Roles.FirstAsync();
        var userC = await BattleTestSupport.CreateUserAsync(db, "userC", role.Id);

        db.ChatMessages.AddRange(
            new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "salam" },
            new ChatMessage { SenderId = userA.Id, RecipientId = userC.Id, Content = "unrelated" });
        await db.SaveChangesAsync();

        var handler = new ClearConversationCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());
        await handler.Handle(new ClearConversationCommand(userB.Id), CancellationToken.None);

        Assert.Equal(1, await db.ChatMessages.CountAsync());
        Assert.True(await db.ChatMessages.AnyAsync(m => m.RecipientId == userC.Id));
    }
}
