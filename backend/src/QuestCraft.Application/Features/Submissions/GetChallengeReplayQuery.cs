using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Submissions;

public record ChallengeReplayAttemptDto(int Id, string Verdict, DateTime SubmittedAt, int ExecutionTimeMs, int? SolveTimeMs);

public record ChallengeReplayDto(
    int TotalAttempts,
    int WrongAttempts,
    DateTime? FirstSubmittedAt,
    DateTime? FirstAcceptedAt,
    int? TimeToSolveMs,
    List<ChallengeReplayAttemptDto> Attempts);

public record GetChallengeReplayQuery(int ChallengeId) : IQuery<ChallengeReplayDto>;

public class GetChallengeReplayQueryHandler : IRequestHandler<GetChallengeReplayQuery, ChallengeReplayDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChallengeReplayQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChallengeReplayDto> Handle(GetChallengeReplayQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new ChallengeReplayDto(0, 0, null, null, null, []);
        }

        var submissions = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.ChallengeId == request.ChallengeId)
            .OrderBy(s => s.SubmittedAt)
            .Select(s => new { s.Id, s.Verdict, s.SubmittedAt, s.ExecutionTimeMs, s.SolveTimeMs })
            .ToListAsync(cancellationToken);

        if (submissions.Count == 0)
        {
            return new ChallengeReplayDto(0, 0, null, null, null, []);
        }

        var firstSubmittedAt = submissions[0].SubmittedAt;
        var firstAccepted = submissions.FirstOrDefault(s => s.Verdict == SubmissionVerdict.Accepted);
        var wrongAttempts = submissions
            .TakeWhile(s => firstAccepted is null || s.SubmittedAt < firstAccepted.SubmittedAt)
            .Count(s => s.Verdict != SubmissionVerdict.Accepted);

        int? timeToSolveMs = null;
        if (firstAccepted is not null)
        {
            timeToSolveMs = firstAccepted.SolveTimeMs
                ?? (int)(firstAccepted.SubmittedAt - firstSubmittedAt).TotalMilliseconds;
        }

        var attempts = submissions
            .OrderByDescending(s => s.SubmittedAt)
            .Take(50)
            .Select(s => new ChallengeReplayAttemptDto(s.Id, s.Verdict.ToString(), s.SubmittedAt, s.ExecutionTimeMs, s.SolveTimeMs))
            .ToList();

        return new ChallengeReplayDto(
            submissions.Count,
            wrongAttempts,
            firstSubmittedAt,
            firstAccepted?.SubmittedAt,
            timeToSolveMs,
            attempts);
    }
}
