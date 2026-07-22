using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record CancelBattleCommand(int BattleId) : ICommand<Unit>;

public class CancelBattleCommandHandler : IRequestHandler<CancelBattleCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IBattleHubNotifier _battleHubNotifier;

    public CancelBattleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IBattleHubNotifier battleHubNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _battleHubNotifier = battleHubNotifier;
    }

    public async Task<Unit> Handle(CancelBattleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var battle = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedTitle)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedBadge)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.BattleId);

        if (battle.HostUserId != userId)
        {
            throw new ForbiddenException("Yalnız otağın sahibi döyüşü ləğv edə bilər.");
        }

        if (battle.Status != BattleStatus.Waiting)
        {
            throw new ConflictException("Bu döyüş artıq ləğv edilə bilməz.");
        }

        battle.Status = BattleStatus.Cancelled;
        battle.EndedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        await _battleHubNotifier.NotifyBattleUpdated(battle.Id, BattleMapper.ToDto(battle, _currentUser.IsEnglish), cancellationToken);

        return Unit.Value;
    }
}
