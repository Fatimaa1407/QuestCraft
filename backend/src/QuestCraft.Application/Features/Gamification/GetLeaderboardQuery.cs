using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record LeaderboardEntryDto(int Rank, int UserId, string Username, string? AvatarUrl, int Xp, int Level);

public record GetLeaderboardQuery(LeaderboardPeriod Period, int Top) : IQuery<List<LeaderboardEntryDto>>;

// AllTime reads UserProfile.Xp directly. Daily/Weekly/Monthly sum the XpTransaction log within the
// period (covers challenge, achievement and daily-quest XP alike) — computed live rather than via a
// precomputed snapshot table, which is simple and accurate at the scale this project runs at
// (see docs/ARCHITECTURE.md §16 for the snapshot alternative).
public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, List<LeaderboardEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public GetLeaderboardQueryHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<List<LeaderboardEntryDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"leaderboard:{request.Period}:{request.Top}";
        if (_cache.TryGetValue(cacheKey, out List<LeaderboardEntryDto>? cached) && cached is not null)
        {
            return cached;
        }

        var result = await LoadAsync(request, cancellationToken);
        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));
        return result;
    }

    // XP changes every time someone submits — a 30s cache keeps the board from hammering the DB on
    // every page view while staying close enough to real-time that nobody notices the staleness.
    private async Task<List<LeaderboardEntryDto>> LoadAsync(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        if (request.Period == LeaderboardPeriod.AllTime)
        {
            var allTime = await _context.UserProfiles
                .Include(p => p.User)
                .OrderByDescending(p => p.Xp)
                .Take(request.Top)
                .Select(p => new { p.UserId, p.User.Username, p.AvatarUrl, p.Xp, p.Level })
                .ToListAsync(cancellationToken);

            return allTime.Select((p, i) => new LeaderboardEntryDto(i + 1, p.UserId, p.Username, p.AvatarUrl, p.Xp, p.Level)).ToList();
        }

        var now = DateTime.UtcNow;
        var periodStart = request.Period switch
        {
            LeaderboardPeriod.Daily => now.Date,
            LeaderboardPeriod.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            LeaderboardPeriod.Monthly => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => now.Date,
        };

        var grouped = await _context.XpTransactions
            .Where(x => x.EarnedAt >= periodStart)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Xp = g.Sum(x => x.Amount) })
            .OrderByDescending(g => g.Xp)
            .Take(request.Top)
            .ToListAsync(cancellationToken);

        var userIds = grouped.Select(g => g.UserId).ToList();
        var profiles = await _context.UserProfiles
            .Include(p => p.User)
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        return grouped
            .Where(g => profiles.ContainsKey(g.UserId))
            .Select((g, i) => new LeaderboardEntryDto(
                i + 1, g.UserId, profiles[g.UserId].User.Username, profiles[g.UserId].AvatarUrl, g.Xp, profiles[g.UserId].Level))
            .ToList();
    }
}
