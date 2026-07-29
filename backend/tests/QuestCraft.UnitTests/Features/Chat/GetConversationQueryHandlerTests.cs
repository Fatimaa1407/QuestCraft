using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Chat;
using QuestCraft.Domain.Entities;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

public class GetConversationQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyMessagesBetweenTheTwoUsers()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var role = await db.Roles.FirstAsync();
        var userC = await BattleTestSupport.CreateUserAsync(db, "userC", role.Id);

        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "a-to-b" });
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "b-to-a" });
        // A conversation with a third party must never leak into A/B's thread — this is the
        // authorization boundary the audit specifically checked (can user A read someone else's DM).
        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userC.Id, Content = "a-to-c" });
        await db.SaveChangesAsync();

        var handler = new GetConversationQueryHandler(db, new FakeCurrentUserService { UserId = userA.Id });
        var result = await handler.Handle(new GetConversationQuery(userB.Id, 1, 30), CancellationToken.None);

        Assert.Equal(2, result.Items.Count);
        Assert.DoesNotContain(result.Items, m => m.Content == "a-to-c");
    }
}
