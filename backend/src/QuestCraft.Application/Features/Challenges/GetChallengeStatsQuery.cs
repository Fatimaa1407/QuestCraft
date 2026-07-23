using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Challenges;

public record ChallengeStatsDto(int SolverCount, int TotalSubmissions, double AcceptanceRate, int? AverageSolveTimeMs);

public record GetChallengeStatsQuery(int ChallengeId) : IQuery<ChallengeStatsDto>;

public class GetChallengeStatsQueryHandler : IRequestHandler<GetChallengeStatsQuery, ChallengeStatsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public GetChallengeStatsQueryHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    public async Task<ChallengeStatsDto> Handle(GetChallengeStatsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"challenge-stats:{request.ChallengeId}";
        if (_cache.TryGetValue(cacheKey, out ChallengeStatsDto? cached) && cached is not null)
        {
            return cached;
        }

        var submissions = await _context.ChallengeSubmissions
            .Where(s => s.ChallengeId == request.ChallengeId)
            .Select(s => new { s.UserId, s.Verdict, s.SolveTimeMs })
            .ToListAsync(cancellationToken);

        var totalSubmissions = submissions.Count;
        var solverCount = submissions.Where(s => s.Verdict == SubmissionVerdict.Accepted).Select(s => s.UserId).Distinct().Count();
        var acceptedCount = submissions.Count(s => s.Verdict == SubmissionVerdict.Accepted);
        var acceptanceRate = totalSubmissions == 0 ? 0 : (double)acceptedCount / totalSubmissions;

        var solveTimes = submissions
            .Where(s => s.Verdict == SubmissionVerdict.Accepted && s.SolveTimeMs != null)
            .Select(s => s.SolveTimeMs!.Value)
            .ToList();
        int? averageSolveTimeMs = solveTimes.Count == 0 ? null : (int)solveTimes.Average();

        var result = new ChallengeStatsDto(solverCount, totalSubmissions, acceptanceRate, averageSolveTimeMs);
        _cache.Set(cacheKey, result, TimeSpan.FromSeconds(60));
        return result;
    }
}
