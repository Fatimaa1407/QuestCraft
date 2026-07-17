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
}
