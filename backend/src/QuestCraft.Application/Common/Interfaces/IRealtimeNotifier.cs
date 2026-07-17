namespace QuestCraft.Application.Common.Interfaces;

/// <summary>
/// Pings a connected client that it has new notification data to fetch. Deliberately carries no
/// payload — the client just refetches its notification list/unread-count over the existing REST
/// API, keeping the realtime channel decoupled from each feature's notification DTO shape.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyNewNotification(int userId, CancellationToken cancellationToken = default);
}
