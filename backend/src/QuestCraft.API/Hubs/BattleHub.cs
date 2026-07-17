using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace QuestCraft.API.Hubs;

[Authorize]
public class BattleHub : Hub
{
    public static string GroupNameForBattle(int battleId) => $"battle-{battleId}";

    // Clients call this after loading the battle room page to subscribe to that battle's live
    // updates — there's no server-side way to know which battle a connection cares about otherwise.
    public Task JoinBattleGroup(int battleId) => Groups.AddToGroupAsync(Context.ConnectionId, GroupNameForBattle(battleId));

    public Task LeaveBattleGroup(int battleId) => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupNameForBattle(battleId));
}
