using QuestCraft.Application.Features.Chat;

namespace QuestCraft.Application.Common.Interfaces;

public interface IRealtimeNotifier
{
    /// <summary>
    /// Pings a connected client that it has new notification data to fetch. Deliberately carries no
    /// payload — the client just refetches its notification list/unread-count over the existing REST
    /// API, keeping the realtime channel decoupled from each feature's notification DTO shape.
    /// </summary>
    Task NotifyNewNotification(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pushes the actual chat message to the recipient — unlike NotifyNewNotification, chat needs the
    /// real content delivered instantly so the thread updates without a round-trip refetch.
    /// </summary>
    Task NotifyChatMessage(int recipientUserId, ChatMessageDto message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Tells the other party a specific message they can see was deleted, so their thread can drop it
    /// live. senderId identifies which conversation thread it belonged to (only a message's own
    /// sender can delete it, so this is always the deleting user's id).
    /// </summary>
    Task NotifyChatMessageDeleted(int recipientUserId, int messageId, int senderId, CancellationToken cancellationToken = default);

    /// <summary>Tells the other party their shared history with clearedByUserId was cleared, so their thread can empty out live.</summary>
    Task NotifyConversationCleared(int recipientUserId, int clearedByUserId, CancellationToken cancellationToken = default);
}
