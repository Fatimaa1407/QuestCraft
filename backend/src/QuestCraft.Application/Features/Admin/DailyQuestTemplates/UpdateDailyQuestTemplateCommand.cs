using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record UpdateDailyQuestTemplateCommand(
    int Id, string Title, string? TitleEn, string? Description, string? DescriptionEn,
    string TargetType, int TargetValue, int XpReward, int CoinReward, bool IsActive) : ICommand<DailyQuestTemplateAdminDto>;

public class UpdateDailyQuestTemplateCommandValidator : AbstractValidator<UpdateDailyQuestTemplateCommand>
{
    public UpdateDailyQuestTemplateCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlıq boş ola bilməz.").MaximumLength(150);
        RuleFor(x => x.TargetType).Must(v => Enum.TryParse<DailyQuestTargetType>(v, out _))
            .WithMessage("Etibarsız hədəf tipi.");
        RuleFor(x => x.TargetValue).GreaterThan(0);
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CoinReward).GreaterThanOrEqualTo(0);
    }
}

public class UpdateDailyQuestTemplateCommandHandler : IRequestHandler<UpdateDailyQuestTemplateCommand, DailyQuestTemplateAdminDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateDailyQuestTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyQuestTemplateAdminDto> Handle(UpdateDailyQuestTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _context.DailyQuestTemplates.FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(DailyQuestTemplate), request.Id);

        template.Title = request.Title;
        template.TitleEn = request.TitleEn;
        template.Description = request.Description;
        template.DescriptionEn = request.DescriptionEn;
        template.TargetType = Enum.Parse<DailyQuestTargetType>(request.TargetType);
        template.TargetValue = request.TargetValue;
        template.XpReward = request.XpReward;
        template.CoinReward = request.CoinReward;
        template.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new DailyQuestTemplateAdminDto(
            template.Id, template.Title, template.TitleEn, template.Description, template.DescriptionEn,
            template.TargetType.ToString(), template.TargetValue, template.XpReward, template.CoinReward, template.IsActive);
    }
}
