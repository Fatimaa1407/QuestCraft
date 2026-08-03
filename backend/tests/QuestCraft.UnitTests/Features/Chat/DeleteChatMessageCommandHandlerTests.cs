using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Chat;
using QuestCraft.Domain.Entities;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Chat;

public class DeleteChatMessageCommandHandlerTests
{
    [Fact]
    public async Task Handle_OwnMessage_DeletesIt()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var message = new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "salam" };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        var handler = new DeleteChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier());
        await handler.Handle(new DeleteChatMessageCommand(message.Id), CancellationToken.None);

        Assert.False(await db.ChatMessages.AnyAsync(m => m.Id == message.Id));
    }

    [Fact]
    public async Task Handle_SomeoneElsesMessage_ThrowsForbidden()
    {
        var (db, userA, userB) = await ChatFriendsTestSupport.SeedTwoUsersAsync();
        var message = new ChatMessage { SenderId = userA.Id, RecipientId = userB.Id, Content = "salam" };
        db.ChatMessages.Add(message);
        await db.SaveChangesAsync();

        // The recipient trying to delete a message they merely received, not sent.
        var handler = new DeleteChatMessageCommandHandler(db, new FakeCurrentUserService { UserId = userB.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new DeleteChatMessageCommand(message.Id), CancellationToken.None));
    }
}
