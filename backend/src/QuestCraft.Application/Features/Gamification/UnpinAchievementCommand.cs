using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Gamification;

public record UnpinAchievementCommand(int AchievementId) : ICommand<Unit>;

public class UnpinAchievementCommandHandler : IRequestHandler<UnpinAchievementCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UnpinAchievementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(UnpinAchievementCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == request.AchievementId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserAchievement), request.AchievementId);

        userAchievement.IsPinned = false;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
