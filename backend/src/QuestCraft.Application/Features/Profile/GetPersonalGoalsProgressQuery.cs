using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Profile;

public record PersonalGoalsProgressDto(
    int? ChallengeGoal, int ChallengesDoneToday,
    int? XpGoal, int XpToday,
    int? BattleGoal, int BattlesToday);

public record GetPersonalGoalsProgressQuery : IQuery<PersonalGoalsProgressDto>;

public class GetPersonalGoalsProgressQueryHandler : IRequestHandler<GetPersonalGoalsProgressQuery, PersonalGoalsProgressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPersonalGoalsProgressQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PersonalGoalsProgressDto> Handle(GetPersonalGoalsProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
        {
            return new PersonalGoalsProgressDto(null, 0, null, 0, null, 0);
        }

        var profile = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => new { p.DailyGoalChallenges, p.DailyGoalXp, p.DailyGoalBattles })
            .FirstOrDefaultAsync(cancellationToken);

        var todayStart = DateOnly.FromDateTime(DateTime.UtcNow).ToDateTime(TimeOnly.MinValue);

        var challengesDoneToday = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted && s.SubmittedAt >= todayStart)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var xpToday = await _context.XpTransactions
            .Where(t => t.UserId == userId && t.EarnedAt >= todayStart)
            .SumAsync(t => (int?)t.Amount, cancellationToken) ?? 0;

        var battlesToday = await _context.BattleParticipants
            .CountAsync(p => p.UserId == userId && p.HasFinished && p.FinishedAt >= todayStart, cancellationToken);

        return new PersonalGoalsProgressDto(
            profile?.DailyGoalChallenges, challengesDoneToday,
            profile?.DailyGoalXp, xpToday,
            profile?.DailyGoalBattles, battlesToday);
    }
}
