using MediatR;
using QuestCraft.Application.Features.Battles;

namespace QuestCraft.API.Services;

// Ticks every couple of minutes and force-resolves any battle that's been sitting in Waiting or
// InProgress past its timeout (see AutoResolveStaleBattlesCommand) — without this, an abandoned
// room or an unfinished duel would stay open forever with no automatic resolution.
public class BattleTimeoutBackgroundService : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromMinutes(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BattleTimeoutBackgroundService> _logger;

    public BattleTimeoutBackgroundService(IServiceScopeFactory scopeFactory, ILogger<BattleTimeoutBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                var resolvedCount = await mediator.Send(new AutoResolveStaleBattlesCommand(), stoppingToken);
                if (resolvedCount > 0)
                {
                    _logger.LogInformation("Battle timeout sweep: resolved {Count} stale battles.", resolvedCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Battle timeout background tick failed.");
            }

            try
            {
                await Task.Delay(TickInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }
}
