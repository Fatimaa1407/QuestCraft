using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record XpDayPointDto(DateOnly Date, int Xp);
public record CategoryProgressDto(string CategoryName, int Completed, int Total);
public record DashboardAnalyticsDto(
    List<XpDayPointDto> XpLast30Days,
    List<CategoryProgressDto> CategoryProgress,
    int ActiveDaysLast30);

public record GetDashboardAnalyticsQuery : IQuery<DashboardAnalyticsDto>;

public class GetDashboardAnalyticsQueryHandler : IRequestHandler<GetDashboardAnalyticsQuery, DashboardAnalyticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IMemoryCache _cache;

    public GetDashboardAnalyticsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, IMemoryCache cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<DashboardAnalyticsDto> Handle(GetDashboardAnalyticsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new DashboardAnalyticsDto([], [], 0);
        }

        var cacheKey = $"dashboard-analytics:{userId}";
        if (_cache.TryGetValue(cacheKey, out DashboardAnalyticsDto? cached) && cached is not null)
        {
            return cached;
        }

        var result = await LoadAsync(userId.Value, cancellationToken);
        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));
        return result;
    }

    private async Task<DashboardAnalyticsDto> LoadAsync(int userId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-29);
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);

        var xpRaw = await _context.XpTransactions
            .Where(t => t.UserId == userId && t.EarnedAt >= startDateTime)
            .Select(t => new { t.EarnedAt, t.Amount })
            .ToListAsync(cancellationToken);

        var xpByDate = xpRaw
            .GroupBy(t => DateOnly.FromDateTime(t.EarnedAt))
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var xpLast30Days = new List<XpDayPointDto>();
        for (var d = startDate; d <= today; d = d.AddDays(1))
        {
            xpLast30Days.Add(new XpDayPointDto(d, xpByDate.GetValueOrDefault(d, 0)));
        }

        var userLevel = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.Level)
            .FirstOrDefaultAsync(cancellationToken);
        if (userLevel == 0)
        {
            userLevel = 1;
        }

        var totalsByCategory = await _context.Challenges
            .Where(c => c.IsPublished && c.RequiredLevel <= userLevel)
            .GroupBy(c => new { c.CategoryId, c.Category.Name })
            .Select(g => new { g.Key.CategoryId, g.Key.Name, Total = g.Count() })
            .ToListAsync(cancellationToken);

        var completedChallenges = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted
                && s.Challenge.IsPublished && s.Challenge.RequiredLevel <= userLevel)
            .Select(s => new { s.Challenge.CategoryId, s.ChallengeId })
            .Distinct()
            .ToListAsync(cancellationToken);

        var completedCountByCategory = completedChallenges
            .GroupBy(x => x.CategoryId)
            .ToDictionary(g => g.Key, g => g.Count());

        var categoryProgress = totalsByCategory
            .Where(t => t.Total > 0)
            .Select(t => new CategoryProgressDto(t.Name, completedCountByCategory.GetValueOrDefault(t.CategoryId, 0), t.Total))
            .ToList();

        var activeDaysLast30 = await _context.ActivityLogs
            .CountAsync(a => a.UserId == userId && a.ActionCount > 0 && a.ActivityDate >= startDate && a.ActivityDate <= today, cancellationToken);

        return new DashboardAnalyticsDto(xpLast30Days, categoryProgress, activeDaysLast30);
    }
}
