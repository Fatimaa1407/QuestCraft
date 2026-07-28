using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record LevelProgressDto(
    int Level,
    int ChallengesCompleted,
    int ChallengesTotal,
    int QuizzesCompleted,
    int QuizzesTotal,
    int OverallCompleted,
    int OverallTotal,
    bool IsMaxLevel);

public record GetLevelProgressQuery : IQuery<LevelProgressDto>;

public class GetLevelProgressQueryHandler : IRequestHandler<GetLevelProgressQuery, LevelProgressDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IContentCompletionService _completionService;

    public GetLevelProgressQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, IContentCompletionService completionService)
    {
        _context = context;
        _currentUser = currentUser;
        _completionService = completionService;
    }

    public async Task<LevelProgressDto> Handle(GetLevelProgressQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        var level = profile?.Level ?? 1;
        if (level == 0)
        {
            level = 1;
        }

        var completion = await _completionService.GetLevelCompletionAsync(userId, level, cancellationToken);

        // Self-healing safety net: the stored Level is normally advanced as a side effect of
        // submitting a challenge/quiz (see SubmitChallengeCommand/SubmitQuizAttemptCommand), but if
        // it ever drifts behind the user's actual completion — e.g. a content re-categorization
        // changes what counts toward a level after the user already finished it, with no new
        // submission left to re-trigger the check — just viewing this panel corrects it here too,
        // so a stale Level can never persist indefinitely without the user needing to act first.
        if (completion.IsComplete && profile is not null)
        {
            var unlockedLevel = await _completionService.CalculateUnlockedLevelAsync(userId, cancellationToken);
            if (unlockedLevel > level)
            {
                profile.Level = unlockedLevel;
                await _context.SaveChangesAsync(cancellationToken);
                level = unlockedLevel;
                completion = await _completionService.GetLevelCompletionAsync(userId, level, cancellationToken);
            }
        }

        return new LevelProgressDto(
            level,
            completion.ChallengesCompleted,
            completion.ChallengesTotal,
            completion.QuizzesCompleted,
            completion.QuizzesTotal,
            completion.TotalCompleted,
            completion.TotalItems,
            completion.TotalItems == 0);
    }
}
