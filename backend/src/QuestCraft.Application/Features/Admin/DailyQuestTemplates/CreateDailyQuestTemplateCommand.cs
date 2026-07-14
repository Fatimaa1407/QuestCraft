using FluentValidation;
using MediatR;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record CreateDailyQuestTemplateCommand(
    string Title, string? TitleEn, string? Description, string? DescriptionEn,
    string TargetType, int TargetValue, int XpReward, int CoinReward, bool IsActive) : ICommand<DailyQuestTemplateAdminDto>;

public class CreateDailyQuestTemplateCommandValidator : AbstractValidator<CreateDailyQuestTemplateCommand>
{
    public CreateDailyQuestTemplateCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Başlıq boş ola bilməz.").MaximumLength(150);
        RuleFor(x => x.TargetType).Must(v => Enum.TryParse<DailyQuestTargetType>(v, out _))
            .WithMessage("Etibarsız hədəf tipi.");
        RuleFor(x => x.TargetValue).GreaterThan(0);
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CoinReward).GreaterThanOrEqualTo(0);
    }
}

public class CreateDailyQuestTemplateCommandHandler : IRequestHandler<CreateDailyQuestTemplateCommand, DailyQuestTemplateAdminDto>
{
    private readonly IApplicationDbContext _context;

    public CreateDailyQuestTemplateCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyQuestTemplateAdminDto> Handle(CreateDailyQuestTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new DailyQuestTemplate
        {
            Title = request.Title,
            TitleEn = request.TitleEn,
            Description = request.Description,
            DescriptionEn = request.DescriptionEn,
            TargetType = Enum.Parse<DailyQuestTargetType>(request.TargetType),
            TargetValue = request.TargetValue,
            XpReward = request.XpReward,
            CoinReward = request.CoinReward,
            IsActive = request.IsActive,
        };

        _context.DailyQuestTemplates.Add(template);
        await _context.SaveChangesAsync(cancellationToken);

        return new DailyQuestTemplateAdminDto(
            template.Id, template.Title, template.TitleEn, template.Description, template.DescriptionEn,
            template.TargetType.ToString(), template.TargetValue, template.XpReward, template.CoinReward, template.IsActive);
    }
}
