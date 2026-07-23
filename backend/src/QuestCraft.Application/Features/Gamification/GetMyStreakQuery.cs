using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record HeatmapDayDto(DateOnly Date, int Count);

public record StreakDto(int CurrentStreak, int LongestStreak, DateOnly? LastActivityDate, List<HeatmapDayDto> ActiveDatesLast30);

public record GetMyStreakQuery : IQuery<StreakDto>;

public class GetMyStreakQueryHandler : IRequestHandler<GetMyStreakQuery, StreakDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyStreakQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<StreakDto> Handle(GetMyStreakQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new StreakDto(0, 0, null, []);
        }

        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-29);

        var activeDatesLast30 = await _context.ActivityLogs
            .Where(a => a.UserId == userId && a.ActionCount > 0 && a.ActivityDate >= startDate && a.ActivityDate <= today)
            .OrderBy(a => a.ActivityDate)
            .Select(a => new HeatmapDayDto(a.ActivityDate, a.ActionCount))
            .ToListAsync(cancellationToken);

        return new StreakDto(
            streak?.CurrentStreak ?? 0,
            streak?.LongestStreak ?? 0,
            streak?.LastActivityDate,
            activeDatesLast30);
    }
}
