using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

// Battles have no other time-based resolution: a Room nobody starts, or a Duel invite nobody
// accepts, would otherwise sit in Waiting forever (and keep cluttering GetOpenRoomsQuery); a battle
// where nobody ever finishes would sit InProgress forever with no winner ever declared. This sweeps
// both away on a schedule (see BattleTimeoutBackgroundService) rather than requiring a real-time
// countdown mechanism — simplest thing that works at this project's scale.
public record AutoResolveStaleBattlesCommand : ICommand<int>;

public class AutoResolveStaleBattlesCommandHandler : IRequestHandler<AutoResolveStaleBattlesCommand, int>
{
    public static readonly TimeSpan WaitingTimeout = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan InProgressTimeout = TimeSpan.FromMinutes(20);

    private readonly IApplicationDbContext _context;
    private readonly IBattleHubNotifier _battleHubNotifier;
    private readonly IAchievementEvaluator _achievementEvaluator;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public AutoResolveStaleBattlesCommandHandler(
        IApplicationDbContext context, IBattleHubNotifier battleHubNotifier, IAchievementEvaluator achievementEvaluator, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _battleHubNotifier = battleHubNotifier;
        _achievementEvaluator = achievementEvaluator;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<int> Handle(AutoResolveStaleBattlesCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var resolvedCount = 0;

        var staleWaiting = await _context.Battles
            .Where(b => b.Status == BattleStatus.Waiting && b.CreatedAt < now - WaitingTimeout)
            .ToListAsync(cancellationToken);

        foreach (var battle in staleWaiting)
        {
            battle.Status = BattleStatus.Cancelled;
            battle.EndedAt = now;
            resolvedCount++;
        }

        var staleInProgress = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Statistics)
            .Where(b => b.Status == BattleStatus.InProgress && b.StartedAt != null && b.StartedAt < now - InProgressTimeout)
            .ToListAsync(cancellationToken);

        var winnersToNotify = new List<int>();
        foreach (var battle in staleInProgress)
        {
            battle.Status = BattleStatus.Finished;
            battle.EndedAt = now;
            BattleFinalizer.RankRemaining(battle, startingRank: 1);
            BattleFinalizer.FlagSuspiciousSimilarity(_context, battle);

            var winnerId = BattleFinalizer.GrantWinnerReward(_context, battle);
            if (winnerId is not null)
            {
                winnersToNotify.Add(winnerId.Value);
            }

            resolvedCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);

        foreach (var battle in staleWaiting.Concat(staleInProgress))
        {
            var fresh = await _context.Battles
                .Include(b => b.Challenge)
                .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
                .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
                .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
                .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
                .FirstAsync(b => b.Id == battle.Id, cancellationToken);
            await _battleHubNotifier.NotifyBattleUpdated(battle.Id, BattleMapper.ToDto(fresh), cancellationToken);
        }

        foreach (var winnerId in winnersToNotify)
        {
            await _achievementEvaluator.EvaluateAsync(winnerId, cancellationToken);
        }
        await _context.SaveChangesAsync(cancellationToken);

        foreach (var winnerId in winnersToNotify)
        {
            await _realtimeNotifier.NotifyNewNotification(winnerId, cancellationToken);
        }

        return resolvedCount;
    }
}
