using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Achievements;

public record RestoreAchievementCommand(int Id) : ICommand<AchievementAdminDto>;

public class RestoreAchievementCommandHandler : IRequestHandler<RestoreAchievementCommand, AchievementAdminDto>
{
    private readonly IApplicationDbContext _context;

    public RestoreAchievementCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AchievementAdminDto> Handle(RestoreAchievementCommand request, CancellationToken cancellationToken)
    {
        var achievement = await _context.Achievements
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.Id == request.Id && a.IsDeleted, cancellationToken)
            ?? throw new NotFoundException(nameof(Achievement), request.Id);

        achievement.IsDeleted = false;
        await _context.SaveChangesAsync(cancellationToken);

        return new AchievementAdminDto(
            achievement.Id, achievement.Name, achievement.NameEn, achievement.Description, achievement.DescriptionEn,
            achievement.IconUrl, achievement.ConditionType.ToString(), achievement.ConditionValue,
            achievement.XpReward, achievement.CoinReward, achievement.IsActive);
    }
}
