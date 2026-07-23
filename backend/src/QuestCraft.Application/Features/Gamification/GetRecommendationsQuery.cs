using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record RecommendedChallengeDto(int Id, string Title, string Difficulty, int XpReward);

public record RecommendationDto(string? WeakCategoryName, double? AcceptanceRate, List<RecommendedChallengeDto> Challenges);

public record GetRecommendationsQuery : IQuery<RecommendationDto>;

public class GetRecommendationsQueryHandler : IRequestHandler<GetRecommendationsQuery, RecommendationDto>
{
    private const int MinSubmissionsForSignal = 3;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetRecommendationsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<RecommendationDto> Handle(GetRecommendationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new RecommendationDto(null, null, []);
        }

        var userLevel = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.Level)
            .FirstOrDefaultAsync(cancellationToken);
        if (userLevel == 0)
        {
            userLevel = 1;
        }

        var submissions = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId)
            .Select(s => new { s.Verdict, s.Challenge.CategoryId, CategoryName = s.Challenge.Category.Name })
            .ToListAsync(cancellationToken);

        var byCategory = submissions
            .GroupBy(s => new { s.CategoryId, s.CategoryName })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                Total = g.Count(),
                Accepted = g.Count(s => s.Verdict == SubmissionVerdict.Accepted),
            })
            .Where(g => g.Total >= MinSubmissionsForSignal)
            .Select(g => new { g.CategoryId, g.CategoryName, Rate = (double)g.Accepted / g.Total })
            .ToList();

        if (byCategory.Count == 0)
        {
            return new RecommendationDto(null, null, []);
        }

        var weakest = byCategory.OrderBy(g => g.Rate).First();

        var solvedChallengeIds = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted)
            .Select(s => s.ChallengeId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var recommended = await _context.Challenges
            .Where(c => c.IsPublished && !c.IsBattleOnly && c.CategoryId == weakest.CategoryId
                && c.RequiredLevel <= userLevel && !solvedChallengeIds.Contains(c.Id))
            .OrderBy(c => c.RequiredLevel).ThenBy(c => c.XpReward)
            .Take(3)
            .Select(c => new RecommendedChallengeDto(c.Id, c.Title, c.Difficulty.Name, c.XpReward))
            .ToListAsync(cancellationToken);

        return new RecommendationDto(weakest.CategoryName, weakest.Rate, recommended);
    }
}
