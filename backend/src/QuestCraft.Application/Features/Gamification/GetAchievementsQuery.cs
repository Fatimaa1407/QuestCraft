using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record AchievementDto(
    int Id,
    string Name,
    string Description,
    string? IconUrl,
    int XpReward,
    int CoinReward,
    bool IsUnlocked,
    DateTime? UnlockedAt);

public record GetAchievementsQuery : IQuery<List<AchievementDto>>;

public class GetAchievementsQueryHandler : IRequestHandler<GetAchievementsQuery, List<AchievementDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetAchievementsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<AchievementDto>> Handle(GetAchievementsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var unlockedMap = userId is null
            ? []
            : await _context.UserAchievements
                .Where(ua => ua.UserId == userId)
                .ToDictionaryAsync(ua => ua.AchievementId, ua => ua.UnlockedAt, cancellationToken);

        var achievements = await _context.Achievements
            .Where(a => a.IsActive)
            .OrderBy(a => a.XpReward)
            .ToListAsync(cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        return achievements.Select(a => new AchievementDto(
            a.Id,
            LocalizationHelper.Pick(a.Name, a.NameEn, isEnglish),
            LocalizationHelper.Pick(a.Description, a.DescriptionEn, isEnglish),
            a.IconUrl, a.XpReward, a.CoinReward,
            unlockedMap.ContainsKey(a.Id), unlockedMap.GetValueOrDefault(a.Id)))
            .ToList();
    }
}
