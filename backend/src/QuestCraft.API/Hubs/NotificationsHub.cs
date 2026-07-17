using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QuestCraft.API.Hubs;

[Authorize]
public class NotificationsHub : Hub
{
    public static string GroupNameForUser(int userId) => $"user-{userId}";

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GroupNameForUser(int.Parse(userId)));
        }

        await base.OnConnectedAsync();
    }
}
