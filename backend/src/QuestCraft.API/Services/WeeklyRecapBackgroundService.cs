using MediatR;
using QuestCraft.Application.Features.Gamification;

namespace QuestCraft.API.Services;

// Ticks once a day; the command itself dedupes per-user via the last-sent timestamp, so a daily
// tick is enough to guarantee everyone gets a recap roughly once a week without needing a real
// cron scheduler — simplest thing that works at this project's scale.
public class WeeklyRecapBackgroundService : BackgroundService
{
    private static readonly TimeSpan TickInterval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeeklyRecapBackgroundService> _logger;

    public WeeklyRecapBackgroundService(IServiceScopeFactory scopeFactory, ILogger<WeeklyRecapBackgroundService> logger)
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
                var sentCount = await mediator.Send(new SendWeeklyRecapCommand(), stoppingToken);
                if (sentCount > 0)
                {
                    _logger.LogInformation("Weekly recap: sent {Count} notifications.", sentCount);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Weekly recap background tick failed.");
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
