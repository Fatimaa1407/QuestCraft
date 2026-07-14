using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record UpdateAchievementCommand(
    int Id, string Name, string? NameEn, string Description, string? DescriptionEn, string? IconUrl,
    string ConditionType, int ConditionValue, int XpReward, int CoinReward, bool IsActive) : ICommand<AchievementAdminDto>;

public class UpdateAchievementCommandValidator : AbstractValidator<UpdateAchievementCommand>
{
    public UpdateAchievementCommandValidator()
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

public class UpdateAchievementCommandHandler : IRequestHandler<UpdateAchievementCommand, AchievementAdminDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementAdminDto> Handle(UpdateAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Achievement), request.Id);

        achievement.Name = request.Name;
        achievement.NameEn = request.NameEn;
        achievement.Description = request.Description;
        achievement.DescriptionEn = request.DescriptionEn;
        achievement.IconUrl = request.IconUrl;
        achievement.ConditionType = Enum.Parse<AchievementConditionType>(request.ConditionType);
        achievement.ConditionValue = request.ConditionValue;
        achievement.XpReward = request.XpReward;
        achievement.CoinReward = request.CoinReward;
        achievement.IsActive = request.IsActive;

        await _context.SaveChangesAsync(cancellationToken);

        return new AchievementAdminDto(
            achievement.Id, achievement.Name, achievement.NameEn, achievement.Description, achievement.DescriptionEn,
            achievement.IconUrl, achievement.ConditionType.ToString(), achievement.ConditionValue,
            achievement.XpReward, achievement.CoinReward, achievement.IsActive);
    }
}
