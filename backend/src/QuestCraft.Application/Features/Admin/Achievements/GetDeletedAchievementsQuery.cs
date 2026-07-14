using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record GetDeletedAchievementsQuery : IQuery<List<AchievementAdminDto>>;

public class GetDeletedAchievementsQueryHandler : IRequestHandler<GetDeletedAchievementsQuery, List<AchievementAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedAchievementsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<AchievementAdminDto>> Handle(GetDeletedAchievementsQuery request, CancellationToken cancellationToken) =>
        _context.Achievements
            .IgnoreQueryFilters()
            .Where(a => a.IsDeleted)
            .OrderBy(a => a.Name)
            .Select(a => new AchievementAdminDto(
                a.Id, a.Name, a.NameEn, a.Description, a.DescriptionEn, a.IconUrl,
                a.ConditionType.ToString(), a.ConditionValue, a.XpReward, a.CoinReward, a.IsActive))
            .ToListAsync(cancellationToken);
}
