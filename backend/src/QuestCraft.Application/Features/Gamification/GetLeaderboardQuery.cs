using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record LeaderboardEntryDto(
    int Rank, int UserId, string Username, string? AvatarUrl, int Xp, int Level,
    string? FrameImageUrl, string? TitleText, string? BadgeImageUrl, string? BadgeName);

public record GetLeaderboardQuery(LeaderboardPeriod Period, int Top) : IQuery<List<LeaderboardEntryDto>>;

// AllTime reads UserProfile.Xp directly. Daily/Weekly/Monthly sum the XpTransaction log within the
// period (covers challenge, achievement and daily-quest XP alike) — computed live rather than via a
// precomputed snapshot table, which is simple and accurate at the scale this project runs at
// (see docs/ARCHITECTURE.md §16 for the snapshot alternative).
public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, List<LeaderboardEntryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ICurrentUserService _currentUser;

    public GetLeaderboardQueryHandler(IApplicationDbContext context, IMemoryCache cache, ICurrentUserService currentUser)
    {
        _context = context;
        _cache = cache;
        _currentUser = currentUser;
    }

    public async Task<List<LeaderboardEntryDto>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var isEnglish = _currentUser.IsEnglish;
        // Language is part of the key because TitleText below is localized — a cached AZ response
        // must never be served to an EN request (and vice versa).
        var cacheKey = $"leaderboard:{request.Period}:{request.Top}:{(isEnglish ? "en" : "az")}";
        if (_cache.TryGetValue(cacheKey, out List<LeaderboardEntryDto>? cached) && cached is not null)
        {
            return cached;
        }

        var result = await LoadAsync(request, isEnglish, cancellationToken);
        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));
        return result;
    }

    // XP changes every time someone submits — a 30s cache keeps the board from hammering the DB on
    // every page view while staying close enough to real-time that nobody notices the staleness.
    private async Task<List<LeaderboardEntryDto>> LoadAsync(GetLeaderboardQuery request, bool isEnglish, CancellationToken cancellationToken)
    {
        if (request.Period == LeaderboardPeriod.AllTime)
        {
            var allTime = await _context.UserProfiles
                .Include(p => p.User)
                .OrderByDescending(p => p.Xp)
                .Take(request.Top)
                .Select(p => new
                {
                    p.UserId, p.User.Username, p.AvatarUrl, p.Xp, p.Level,
                    EquippedAvatarUrl = p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : null,
                    FrameImageUrl = p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null,
                    TitleName = p.EquippedTitle != null ? p.EquippedTitle.Name : null,
                    TitleNameEn = p.EquippedTitle != null ? p.EquippedTitle.NameEn : null,
                    BadgeImageUrl = p.EquippedBadge != null ? p.EquippedBadge.ImageUrl : null,
                    BadgeName = p.EquippedBadge != null ? p.EquippedBadge.Name : null,
                    BadgeNameEn = p.EquippedBadge != null ? p.EquippedBadge.NameEn : null,
                })
                .ToListAsync(cancellationToken);

            return allTime.Select((p, i) => new LeaderboardEntryDto(
                i + 1, p.UserId, p.Username, p.EquippedAvatarUrl ?? p.AvatarUrl, p.Xp, p.Level,
                p.FrameImageUrl, LocalizationHelper.PickNullable(p.TitleName, p.TitleNameEn, isEnglish),
                p.BadgeImageUrl, LocalizationHelper.PickNullable(p.BadgeName, p.BadgeNameEn, isEnglish)))
                .ToList();
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
            .Include(p => p.EquippedAvatar)
            .Include(p => p.EquippedFrame)
            .Include(p => p.EquippedTitle)
            .Include(p => p.EquippedBadge)
            .Where(p => userIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, cancellationToken);

        return grouped
            .Where(g => profiles.ContainsKey(g.UserId))
            .Select((g, i) =>
            {
                var profile = profiles[g.UserId];
                return new LeaderboardEntryDto(
                    i + 1, g.UserId, profile.User.Username, profile.EquippedAvatar?.ImageUrl ?? profile.AvatarUrl, g.Xp, profile.Level,
                    profile.EquippedFrame?.ImageUrl,
                    profile.EquippedTitle is null ? null : LocalizationHelper.Pick(profile.EquippedTitle.Name, profile.EquippedTitle.NameEn, isEnglish),
                    profile.EquippedBadge?.ImageUrl,
                    profile.EquippedBadge is null ? null : LocalizationHelper.Pick(profile.EquippedBadge.Name, profile.EquippedBadge.NameEn, isEnglish));
            })
            .ToList();
    }
}
