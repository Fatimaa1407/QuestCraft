using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Chat;

namespace QuestCraft.UnitTests.TestSupport;

public class FakeRealtimeNotifier : IRealtimeNotifier
{
    public List<int> NotifiedUserIds { get; } = [];

    public Task NotifyNewNotification(int userId, CancellationToken cancellationToken = default)
    {
        NotifiedUserIds.Add(userId);
        return Task.CompletedTask;
    }

    public Task NotifyChatMessage(int recipientUserId, ChatMessageDto message, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyChatMessageDeleted(int recipientUserId, int messageId, int senderId, CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task NotifyConversationCleared(int recipientUserId, int clearedByUserId, CancellationToken cancellationToken = default) => Task.CompletedTask;
}
