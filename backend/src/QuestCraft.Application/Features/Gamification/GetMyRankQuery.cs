using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record MyRankDto(
    int Rank, int TotalUsers, int Xp, int Level,
    string? AvatarUrl = null, string? FrameImageUrl = null, string? TitleText = null,
    string? BadgeImageUrl = null, string? BadgeName = null);

public record GetMyRankQuery(LeaderboardPeriod Period) : IQuery<MyRankDto>;

public class GetMyRankQueryHandler : IRequestHandler<GetMyRankQuery, MyRankDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyRankQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MyRankDto> Handle(GetMyRankQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new MyRankDto(0, 0, 0, 1);
        }

        var isEnglish = _currentUser.IsEnglish;
        var cosmetics = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                AvatarUrl = p.EquippedAvatar != null ? p.EquippedAvatar.ImageUrl : p.AvatarUrl,
                FrameImageUrl = p.EquippedFrame != null ? p.EquippedFrame.ImageUrl : null,
                TitleName = p.EquippedTitle != null ? p.EquippedTitle.Name : null,
                TitleNameEn = p.EquippedTitle != null ? p.EquippedTitle.NameEn : null,
                BadgeImageUrl = p.EquippedBadge != null ? p.EquippedBadge.ImageUrl : null,
                BadgeName = p.EquippedBadge != null ? p.EquippedBadge.Name : null,
                BadgeNameEn = p.EquippedBadge != null ? p.EquippedBadge.NameEn : null,
            })
            .FirstOrDefaultAsync(cancellationToken);

        var titleText = LocalizationHelper.PickNullable(cosmetics?.TitleName, cosmetics?.TitleNameEn, isEnglish);
        var badgeName = LocalizationHelper.PickNullable(cosmetics?.BadgeName, cosmetics?.BadgeNameEn, isEnglish);

        if (request.Period == LeaderboardPeriod.AllTime)
        {
            var profile = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => new { p.Xp, p.Level })
                .FirstOrDefaultAsync(cancellationToken);

            var myXp = profile?.Xp ?? 0;
            var myLevel = profile?.Level ?? 1;

            var totalUsers = await _context.UserProfiles.CountAsync(p => p.User.IsActive, cancellationToken);
            // Same tie-break as GetLeaderboardQuery's OrderByDescending(Xp).ThenBy(UserId) — otherwise
            // two users tied on Xp could see a "my rank" number that disagrees with where they'd
            // actually land in the visible list.
            var higherCount = await _context.UserProfiles.CountAsync(
                p => p.User.IsActive && (p.Xp > myXp || (p.Xp == myXp && p.UserId < userId)), cancellationToken);

            return new MyRankDto(higherCount + 1, totalUsers, myXp, myLevel,
                cosmetics?.AvatarUrl, cosmetics?.FrameImageUrl, titleText, cosmetics?.BadgeImageUrl, badgeName);
        }

        var now = DateTime.UtcNow;
        var periodStart = request.Period switch
        {
            LeaderboardPeriod.Daily => now.Date,
            LeaderboardPeriod.Weekly => now.Date.AddDays(-(int)now.DayOfWeek),
            LeaderboardPeriod.Monthly => new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc),
            _ => now.Date,
        };

        var myXpInPeriod = await _context.XpTransactions
            .Where(x => x.UserId == userId && x.EarnedAt >= periodStart)
            .SumAsync(x => (int?)x.Amount, cancellationToken) ?? 0;

        var myLevelInPeriod = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.Level)
            .FirstOrDefaultAsync(cancellationToken);
        if (myLevelInPeriod == 0)
        {
            myLevelInPeriod = 1;
        }

        var totalUsersInPeriod = await _context.XpTransactions
            .Where(x => x.EarnedAt >= periodStart)
            .Select(x => x.UserId)
            .Distinct()
            .CountAsync(cancellationToken);

        // Same tie-break as GetLeaderboardQuery's period path (OrderByDescending(Xp).ThenBy(UserId)).
        var higherCountInPeriod = await _context.XpTransactions
            .Where(x => x.EarnedAt >= periodStart)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Xp = g.Sum(x => x.Amount) })
            .CountAsync(g => g.Xp > myXpInPeriod || (g.Xp == myXpInPeriod && g.UserId < userId), cancellationToken);

        // If the user has no Xp activity in the period, they still count as one of the ranked users
        // (tied for last) rather than being excluded entirely.
        var totalUsers2 = myXpInPeriod > 0 || totalUsersInPeriod == 0 ? totalUsersInPeriod : totalUsersInPeriod + 1;

        return new MyRankDto(higherCountInPeriod + 1, totalUsers2, myXpInPeriod, myLevelInPeriod,
            cosmetics?.AvatarUrl, cosmetics?.FrameImageUrl, titleText, cosmetics?.BadgeImageUrl, badgeName);
    }
}
