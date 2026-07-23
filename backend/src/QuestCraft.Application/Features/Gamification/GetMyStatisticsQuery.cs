using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record MyStatisticsDto(
    int ChallengesSolved,
    int QuizzesCompleted,
    double SuccessRatePercent,
    int? AverageSolveTimeMs,
    int TotalCodingTimeMs,
    int CurrentStreak,
    int LongestStreak,
    int TotalXp,
    int TotalCoinsEarned,
    int BattlesWon,
    int Rank,
    int TotalUsers);

public record GetMyStatisticsQuery : IQuery<MyStatisticsDto>;

// Everything here is read from tables that already exist and are already maintained by other
// handlers (UserStatistics kept up to date by submission/quiz handlers, Streak by
// StreakCalculator, rank computed the same way as GetMyRankQuery's AllTime path) — this query is
// purely a combining read, no new write path.
public class GetMyStatisticsQueryHandler : IRequestHandler<GetMyStatisticsQuery, MyStatisticsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetMyStatisticsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<MyStatisticsDto> Handle(GetMyStatisticsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var streak = await _context.Streaks.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);

        var successRate = stats is not null && stats.TotalSubmissions > 0
            ? Math.Round(stats.AcceptedSubmissions * 100.0 / stats.TotalSubmissions, 1)
            : 0;

        var solveTimes = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.SolveTimeMs != null)
            .Select(s => s.SolveTimeMs!.Value)
            .ToListAsync(cancellationToken);
        int? avgSolveTimeMs = solveTimes.Count > 0 ? (int)solveTimes.Average() : null;
        var totalCodingTimeMs = solveTimes.Sum();

        var battlesWon = await _context.BattleParticipants
            .CountAsync(p => p.UserId == userId && p.Rank == 1 && p.Battle.Status == BattleStatus.Finished, cancellationToken);

        var myXp = profile?.Xp ?? 0;
        var totalUsers = await _context.UserProfiles.CountAsync(cancellationToken);
        var higherCount = await _context.UserProfiles.CountAsync(p => p.Xp > myXp, cancellationToken);

        return new MyStatisticsDto(
            stats?.TotalChallengesSolved ?? 0,
            stats?.TotalQuizzesCompleted ?? 0,
            successRate,
            avgSolveTimeMs,
            totalCodingTimeMs,
            streak?.CurrentStreak ?? 0,
            streak?.LongestStreak ?? 0,
            myXp,
            stats?.TotalCoinsEarned ?? 0,
            battlesWon,
            higherCount + 1,
            totalUsers);
    }
}
