using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Dashboard;

public record AdminDashboardSummaryDto(
    int TotalUsers,
    int TotalChallenges,
    int TotalQuizzes,
    int TotalSubmissions,
    int SubmissionsToday,
    int NewUsersThisWeek,
    int ActiveUsersToday);

public record GetAdminDashboardSummaryQuery : IQuery<AdminDashboardSummaryDto>;

public class GetAdminDashboardSummaryQueryHandler : IRequestHandler<GetAdminDashboardSummaryQuery, AdminDashboardSummaryDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IMemoryCache _cache;

    public GetAdminDashboardSummaryQueryHandler(IApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    private const string CacheKey = "admin-dashboard-summary";

    public async Task<AdminDashboardSummaryDto> Handle(GetAdminDashboardSummaryQuery request, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(CacheKey, out AdminDashboardSummaryDto? cached) && cached is not null)
        {
            return cached;
        }

        var result = await LoadAsync(cancellationToken);
        _cache.Set(CacheKey, result, TimeSpan.FromSeconds(30));
        return result;
    }

    private async Task<AdminDashboardSummaryDto> LoadAsync(CancellationToken cancellationToken)
    {
        var todayStart = DateTime.UtcNow.Date;
        var weekAgo = DateTime.UtcNow.AddDays(-7);

        var totalUsers = await _context.Users.CountAsync(cancellationToken);
        var totalChallenges = await _context.Challenges.CountAsync(cancellationToken);
        var totalQuizzes = await _context.Quizzes.CountAsync(cancellationToken);
        var totalSubmissions = await _context.ChallengeSubmissions.CountAsync(cancellationToken);

        // Counts challenge submissions and quiz attempts together — both represent "activity" for the day.
        var challengeSubmissionsToday = await _context.ChallengeSubmissions
            .CountAsync(s => s.SubmittedAt >= todayStart, cancellationToken);
        var quizAttemptsToday = await _context.QuizAttempts
            .CountAsync(a => a.CompletedAt >= todayStart, cancellationToken);
        var submissionsToday = challengeSubmissionsToday + quizAttemptsToday;

        var newUsersThisWeek = await _context.Users.CountAsync(u => u.CreatedAt >= weekAgo, cancellationToken);

        // "Active today" means logged in today, not "practiced today" — ActivityLog is reserved for the
        // streak system (challenge/quiz submissions only), so a login-only session wouldn't show up there.
        var activeUsersToday = await _context.Users
            .CountAsync(u => u.LastLoginAt != null && u.LastLoginAt >= todayStart, cancellationToken);

        return new AdminDashboardSummaryDto(
            totalUsers,
            totalChallenges,
            totalQuizzes,
            totalSubmissions,
            submissionsToday,
            newUsersThisWeek,
            activeUsersToday);
    }
}
