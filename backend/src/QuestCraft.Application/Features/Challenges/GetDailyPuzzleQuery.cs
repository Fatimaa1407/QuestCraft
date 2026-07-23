using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Challenges;

public record DailyPuzzleDto(int? ChallengeId, string? Title, string? Difficulty, bool SolvedToday);

public record GetDailyPuzzleQuery : IQuery<DailyPuzzleDto>;

public class GetDailyPuzzleQueryHandler : IRequestHandler<GetDailyPuzzleQuery, DailyPuzzleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetDailyPuzzleQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DailyPuzzleDto> Handle(GetDailyPuzzleQuery request, CancellationToken cancellationToken)
    {
        var poolIds = await _context.Challenges
            .Where(c => c.IsDailyPuzzle && c.IsPublished)
            .OrderBy(c => c.Id)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (poolIds.Count == 0)
        {
            return new DailyPuzzleDto(null, null, null, false);
        }

        // Deterministic, shared by every user: rotates to the next pool entry every UTC day, with no
        // stored "today's pick" row and no dependence on Random's internal algorithm.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var index = today.DayNumber % poolIds.Count;
        var challengeId = poolIds[index];

        var challenge = await _context.Challenges
            .Where(c => c.Id == challengeId)
            .Select(c => new { c.Title, DifficultyName = c.Difficulty.Name })
            .FirstAsync(cancellationToken);

        var solvedToday = false;
        var userId = _currentUser.UserId;
        if (userId is not null)
        {
            var todayStart = today.ToDateTime(TimeOnly.MinValue);
            solvedToday = await _context.ChallengeSubmissions.AnyAsync(
                s => s.UserId == userId && s.ChallengeId == challengeId
                    && s.Verdict == SubmissionVerdict.Accepted && s.SubmittedAt >= todayStart,
                cancellationToken);
        }

        return new DailyPuzzleDto(challengeId, challenge.Title, challenge.DifficultyName, solvedToday);
    }
}
