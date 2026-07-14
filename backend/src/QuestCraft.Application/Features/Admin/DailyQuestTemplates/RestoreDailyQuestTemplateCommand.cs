using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record RestoreDailyQuestTemplateCommand(int Id) : ICommand<DailyQuestTemplateAdminDto>;

public class RestoreDailyQuestTemplateCommandHandler : IRequestHandler<RestoreDailyQuestTemplateCommand, DailyQuestTemplateAdminDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreDailyQuestTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyQuestTemplateAdminDto> Handle(RestoreDailyQuestTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.DailyQuestTemplates
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(DailyQuestTemplate), request.Id);

        template.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new DailyQuestTemplateAdminDto(
            template.Id, template.Title, template.TitleEn, template.Description, template.DescriptionEn,
            template.TargetType.ToString(), template.TargetValue, template.XpReward, template.CoinReward, template.IsActive);
    }
}
