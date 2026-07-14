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

        var level = await _context.UserProfiles
            .Where(p => p.UserId == userId)
            .Select(p => p.Level)
            .FirstOrDefaultAsync(cancellationToken);
        if (level == 0)
        {
            level = 1;
        }

        var completion = await _completionService.GetLevelCompletionAsync(userId, level, cancellationToken);

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
