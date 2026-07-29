using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Battles;

namespace QuestCraft.UnitTests.TestSupport;

public class FakeBattleHubNotifier : IBattleHubNotifier
{
    public int CallCount { get; private set; }

    public Task NotifyBattleUpdated(int battleId, BattleDto battle, CancellationToken cancellationToken = default)
    {
        CallCount++;
        return Task.CompletedTask;
    }
}
