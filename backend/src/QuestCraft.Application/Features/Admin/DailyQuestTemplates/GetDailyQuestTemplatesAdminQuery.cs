using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record GetDailyQuestTemplatesAdminQuery : IQuery<List<DailyQuestTemplateAdminDto>>;

public class GetDailyQuestTemplatesAdminQueryHandler : IRequestHandler<GetDailyQuestTemplatesAdminQuery, List<DailyQuestTemplateAdminDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDailyQuestTemplatesAdminQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<DailyQuestTemplateAdminDto>> Handle(GetDailyQuestTemplatesAdminQuery request, CancellationToken cancellationToken) =>
        _context.DailyQuestTemplates
            .OrderBy(t => t.Title)
            .Select(t => new DailyQuestTemplateAdminDto(
                t.Id, t.Title, t.TitleEn, t.Description, t.DescriptionEn,
                t.TargetType.ToString(), t.TargetValue, t.XpReward, t.CoinReward, t.IsActive))
            .ToListAsync(cancellationToken);
}
