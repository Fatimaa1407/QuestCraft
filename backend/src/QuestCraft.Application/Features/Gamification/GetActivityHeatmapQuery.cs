using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record HeatmapDayDetailDto(DateOnly Date, int Count, int ChallengesSolved, int XpEarned, int CodingTimeMs);

public record GetActivityHeatmapQuery(int Days = 180) : IQuery<List<HeatmapDayDetailDto>>;

public class GetActivityHeatmapQueryHandler : IRequestHandler<GetActivityHeatmapQuery, List<HeatmapDayDetailDto>>
{
    private const int MaxDays = 366;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetActivityHeatmapQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<HeatmapDayDetailDto>> Handle(GetActivityHeatmapQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return [];
        }

        var days = Math.Clamp(request.Days, 1, MaxDays);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startDate = today.AddDays(-(days - 1));
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue);

        var countByDate = await _context.ActivityLogs
            .Where(a => a.UserId == userId && a.ActivityDate >= startDate && a.ActivityDate <= today)
            .ToDictionaryAsync(a => a.ActivityDate, a => a.ActionCount, cancellationToken);

        var acceptedSubmissions = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted && s.SubmittedAt >= startDateTime)
            .Select(s => new { s.SubmittedAt, s.SolveTimeMs })
            .ToListAsync(cancellationToken);

        var solvedByDate = acceptedSubmissions
            .GroupBy(s => DateOnly.FromDateTime(s.SubmittedAt))
            .ToDictionary(g => g.Key, g => g.Count());

        var codingTimeByDate = acceptedSubmissions
            .Where(s => s.SolveTimeMs != null)
            .GroupBy(s => DateOnly.FromDateTime(s.SubmittedAt))
            .ToDictionary(g => g.Key, g => g.Sum(s => s.SolveTimeMs!.Value));

        var xpByDate = await _context.XpTransactions
            .Where(t => t.UserId == userId && t.EarnedAt >= startDateTime)
            .Select(t => new { t.EarnedAt, t.Amount })
            .ToListAsync(cancellationToken);
        var xpByDateDict = xpByDate
            .GroupBy(t => DateOnly.FromDateTime(t.EarnedAt))
            .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

        var result = new List<HeatmapDayDetailDto>(days);
        for (var d = startDate; d <= today; d = d.AddDays(1))
        {
            result.Add(new HeatmapDayDetailDto(
                d,
                countByDate.GetValueOrDefault(d, 0),
                solvedByDate.GetValueOrDefault(d, 0),
                xpByDateDict.GetValueOrDefault(d, 0),
                codingTimeByDate.GetValueOrDefault(d, 0)));
        }

        return result;
    }
}
