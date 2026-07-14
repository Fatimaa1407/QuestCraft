using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record GetDeletedDailyQuestTemplatesQuery : IQuery<List<DailyQuestTemplateAdminDto>>;

public class GetDeletedDailyQuestTemplatesQueryHandler : IRequestHandler<GetDeletedDailyQuestTemplatesQuery, List<DailyQuestTemplateAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedDailyQuestTemplatesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<DailyQuestTemplateAdminDto>> Handle(GetDeletedDailyQuestTemplatesQuery request, CancellationToken cancellationToken) =>
        _context.DailyQuestTemplates
            .IgnoreQueryFilters()
            .Where(t => t.IsDeleted)
            .OrderBy(t => t.Title)
            .Select(t => new DailyQuestTemplateAdminDto(
                t.Id, t.Title, t.TitleEn, t.Description, t.DescriptionEn,
                t.TargetType.ToString(), t.TargetValue, t.XpReward, t.CoinReward, t.IsActive))
            .ToListAsync(cancellationToken);
}
