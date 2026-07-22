using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record PinAchievementCommand(int AchievementId) : ICommand<Unit>;

public class PinAchievementCommandHandler : IRequestHandler<PinAchievementCommand, Unit>
{
    private const int MaxPinned = 3;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public PinAchievementCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(PinAchievementCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var userAchievement = await _context.UserAchievements
            .FirstOrDefaultAsync(ua => ua.UserId == userId && ua.AchievementId == request.AchievementId, cancellationToken)
            ?? throw new BadRequestException("Bu nailiyyət hələ açılmayıb.");

        if (userAchievement.IsPinned)
        {
            return Unit.Value;
        }

        var pinnedCount = await _context.UserAchievements.CountAsync(ua => ua.UserId == userId && ua.IsPinned, cancellationToken);
        if (pinnedCount >= MaxPinned)
        {
            throw new ConflictException("Ən çoxu 3 nailiyyət sancıla bilər.");
        }

        userAchievement.IsPinned = true;
        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
