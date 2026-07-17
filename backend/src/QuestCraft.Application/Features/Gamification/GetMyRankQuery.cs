using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record MyRankDto(int Rank, int TotalUsers, int Xp, int Level);

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

        if (request.Period == LeaderboardPeriod.AllTime)
        {
            var profile = await _context.UserProfiles
                .Where(p => p.UserId == userId)
                .Select(p => new { p.Xp, p.Level })
                .FirstOrDefaultAsync(cancellationToken);

            var myXp = profile?.Xp ?? 0;
            var myLevel = profile?.Level ?? 1;

            var totalUsers = await _context.UserProfiles.CountAsync(cancellationToken);
            var higherCount = await _context.UserProfiles.CountAsync(p => p.Xp > myXp, cancellationToken);

            return new MyRankDto(higherCount + 1, totalUsers, myXp, myLevel);
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

        var higherCountInPeriod = await _context.XpTransactions
            .Where(x => x.EarnedAt >= periodStart)
            .GroupBy(x => x.UserId)
            .Select(g => new { UserId = g.Key, Xp = g.Sum(x => x.Amount) })
            .CountAsync(g => g.Xp > myXpInPeriod, cancellationToken);

        // If the user has no Xp activity in the period, they still count as one of the ranked users
        // (tied for last) rather than being excluded entirely.
        var totalUsers2 = myXpInPeriod > 0 || totalUsersInPeriod == 0 ? totalUsersInPeriod : totalUsersInPeriod + 1;

        return new MyRankDto(higherCountInPeriod + 1, totalUsers2, myXpInPeriod, myLevelInPeriod);
    }
}
