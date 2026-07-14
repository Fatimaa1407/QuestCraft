using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record GetAchievementsAdminQuery : IQuery<List<AchievementAdminDto>>;

public class GetAchievementsAdminQueryHandler : IRequestHandler<GetAchievementsAdminQuery, List<AchievementAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAchievementsAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<AchievementAdminDto>> Handle(GetAchievementsAdminQuery request, CancellationToken cancellationToken) =>
        _context.Achievements
            .OrderBy(a => a.XpReward)
            .Select(a => new AchievementAdminDto(
                a.Id, a.Name, a.NameEn, a.Description, a.DescriptionEn, a.IconUrl,
                a.ConditionType.ToString(), a.ConditionValue, a.XpReward, a.CoinReward, a.IsActive))
            .ToListAsync(cancellationToken);
}
