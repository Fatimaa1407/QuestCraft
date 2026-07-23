using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Profile;

public record PersonalGoalsDto(int? DailyGoalChallenges, int? DailyGoalXp, int? DailyGoalBattles);

public record UpdatePersonalGoalsCommand(int? DailyGoalChallenges, int? DailyGoalXp, int? DailyGoalBattles) : ICommand<PersonalGoalsDto>;

public class UpdatePersonalGoalsCommandValidator : AbstractValidator<UpdatePersonalGoalsCommand>
{
    public UpdatePersonalGoalsCommandValidator()
    {
        RuleFor(x => x.DailyGoalChallenges).InclusiveBetween(1, 50)
            .When(x => x.DailyGoalChallenges is not null)
            .WithMessage("Gündəlik challenge məqsədi 1-50 arasında olmalıdır.");
        RuleFor(x => x.DailyGoalXp).InclusiveBetween(1, 5000)
            .When(x => x.DailyGoalXp is not null)
            .WithMessage("Gündəlik XP məqsədi 1-5000 arasında olmalıdır.");
        RuleFor(x => x.DailyGoalBattles).InclusiveBetween(1, 50)
            .When(x => x.DailyGoalBattles is not null)
            .WithMessage("Gündəlik döyüş məqsədi 1-50 arasında olmalıdır.");
    }
}

public class UpdatePersonalGoalsCommandHandler : IRequestHandler<UpdatePersonalGoalsCommand, PersonalGoalsDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdatePersonalGoalsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PersonalGoalsDto> Handle(UpdatePersonalGoalsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("UserProfile", userId);

        profile.DailyGoalChallenges = request.DailyGoalChallenges;
        profile.DailyGoalXp = request.DailyGoalXp;
        profile.DailyGoalBattles = request.DailyGoalBattles;

        await _context.SaveChangesAsync(cancellationToken);

        return new PersonalGoalsDto(profile.DailyGoalChallenges, profile.DailyGoalXp, profile.DailyGoalBattles);
    }
}
