using FluentValidation;
using MediatR;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record CreateAchievementCommand(
    string Name, string? NameEn, string Description, string? DescriptionEn, string? IconUrl,
    string ConditionType, int ConditionValue, int XpReward, int CoinReward, bool IsActive) : ICommand<AchievementAdminDto>;

public class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    public CreateAchievementCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Ad boş ola bilməz.").MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().WithMessage("Təsvir boş ola bilməz.");
        RuleFor(x => x.ConditionType).Must(v => Enum.TryParse<AchievementConditionType>(v, out _))
            .WithMessage("Etibarsız şərt tipi.");
        RuleFor(x => x.ConditionValue).GreaterThan(0);
        RuleFor(x => x.XpReward).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CoinReward).GreaterThanOrEqualTo(0);
    }
}

public class CreateAchievementCommandHandler : IRequestHandler<CreateAchievementCommand, AchievementAdminDto>
{
    private readonly IApplicationDbContext _context;

    public CreateAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementAdminDto> Handle(CreateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = new Achievement
        {
            Name = request.Name,
            NameEn = request.NameEn,
            Description = request.Description,
            DescriptionEn = request.DescriptionEn,
            IconUrl = request.IconUrl,
            ConditionType = Enum.Parse<AchievementConditionType>(request.ConditionType),
            ConditionValue = request.ConditionValue,
            XpReward = request.XpReward,
            CoinReward = request.CoinReward,
            IsActive = request.IsActive,
        };

        _context.Achievements.Add(achievement);
        await _context.SaveChangesAsync(cancellationToken);

        return new AchievementAdminDto(
            achievement.Id, achievement.Name, achievement.NameEn, achievement.Description, achievement.DescriptionEn,
            achievement.IconUrl, achievement.ConditionType.ToString(), achievement.ConditionValue,
            achievement.XpReward, achievement.CoinReward, achievement.IsActive);
    }
}
