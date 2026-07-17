using QuestCraft.Application.Features.Battles;

namespace QuestCraft.Application.Common.Interfaces;

/// <summary>
/// Pushes the full current battle state to everyone subscribed to that battle's live group —
/// simpler than diffing individual join/progress/finish events client-side, and battle payloads
/// are small enough that re-sending the whole thing on every change is cheap.
/// </summary>
public interface IBattleHubNotifier
{
    Task NotifyBattleUpdated(int battleId, BattleDto battle, CancellationToken cancellationToken = default);
}
