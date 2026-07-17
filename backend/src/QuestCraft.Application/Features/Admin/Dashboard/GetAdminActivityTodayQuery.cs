using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Dashboard;

public record AdminActivityItemDto(
    string Kind,
    int UserId,
    string Username,
    string Title,
    string? Verdict,
    int? Score,
    int? TotalQuestions,
    DateTime Timestamp);

public record GetAdminActivityTodayQuery : IQuery<List<AdminActivityItemDto>>;

public class GetAdminActivityTodayQueryHandler : IRequestHandler<GetAdminActivityTodayQuery, List<AdminActivityItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAdminActivityTodayQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AdminActivityItemDto>> Handle(GetAdminActivityTodayQuery request, CancellationToken cancellationToken)
    {
        var todayStart = DateTime.UtcNow.Date;

        var submissions = await _context.ChallengeSubmissions
            .Include(s => s.Challenge)
            .Include(s => s.User)
            .Where(s => s.SubmittedAt >= todayStart)
            .Select(s => new AdminActivityItemDto("Submission", s.UserId, s.User.Username, s.Challenge.Title, s.Verdict.ToString(), null, null, s.SubmittedAt))
            .ToListAsync(cancellationToken);

        var attempts = await _context.QuizAttempts
            .Include(a => a.Quiz)
            .Include(a => a.User)
            .Where(a => a.CompletedAt >= todayStart)
            .Select(a => new AdminActivityItemDto("Quiz", a.UserId, a.User.Username, a.Quiz.Title, null, a.Score, a.TotalQuestions, a.CompletedAt))
            .ToListAsync(cancellationToken);

        return submissions.Concat(attempts).OrderByDescending(a => a.Timestamp).ToList();
    }
}
