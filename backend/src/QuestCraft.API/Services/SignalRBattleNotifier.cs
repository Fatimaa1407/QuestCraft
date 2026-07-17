using Microsoft.AspNetCore.SignalR;
using QuestCraft.API.Hubs;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Battles;

namespace QuestCraft.API.Services;

public class SignalRBattleNotifier : IBattleHubNotifier
{
    private readonly IHubContext<BattleHub> _hubContext;

    public SignalRBattleNotifier(IHubContext<BattleHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task NotifyBattleUpdated(int battleId, BattleDto battle, CancellationToken cancellationToken = default) =>
        _hubContext.Clients.Group(BattleHub.GroupNameForBattle(battleId)).SendAsync("battleUpdated", battle, cancellationToken);
}
