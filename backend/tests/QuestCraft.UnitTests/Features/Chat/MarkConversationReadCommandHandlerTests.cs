using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Chat;
using QuestCraft.Domain.Entities;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

public class MarkConversationReadCommandHandlerTests
{
    [Fact]
    public async Task Handle_MarksOnlyIncomingMessagesAsRead()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        db.ChatMessages.Add(new ChatMessage { SenderId = userB.Id, RecipientId = userA.Id, Content = "hi", IsRead = false });
        db.ChatMessages.Add(new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "hey back", IsRead = false });
        await db.SaveChangesAsync();

        var handler = new MarkConversationReadCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id });
        await handler.Handle(new MarkConversationReadCommand(userB.Id), CancellationToken.None);

        // Only the message userA *received* flips to read — userA's own outgoing message (which only
        // userB can mark read) must stay untouched.
        Assert.True(await db.ChatMessages.AnyAsync(m => m.SenderId == userB.Id && m.IsRead));
        Assert.False(await db.ChatMessages.AnyAsync(m => m.SenderId == userA.Id && m.IsRead));
    }
}
