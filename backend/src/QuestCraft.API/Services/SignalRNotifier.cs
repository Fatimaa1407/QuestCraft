using Microsoft.AspNetCore.SignalR;
using QuestCraft.API.Hubs;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Chat;

namespace QuestCraft.API.Services;

public class SignalRNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationsHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationsHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyNewNotification(int userId, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationsHub.GroupNameForUser(userId)).SendAsync("newNotification", cancellationToken);

    public Task NotifyChatMessage(int recipientUserId, ChatMessageDto message, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(NotificationsHub.GroupNameForUser(recipientUserId)).SendAsync("newChatMessage", message, cancellationToken);
}
