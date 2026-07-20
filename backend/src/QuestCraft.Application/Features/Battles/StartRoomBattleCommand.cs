using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record StartRoomBattleCommand(int BattleId) : ICommand<BattleDto>;

public class StartRoomBattleCommandHandler : IRequestHandler<StartRoomBattleCommand, BattleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IBattleHubNotifier _battleHubNotifier;

    public StartRoomBattleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IBattleHubNotifier battleHubNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _battleHubNotifier = battleHubNotifier;
    }

    public async Task<BattleDto> Handle(StartRoomBattleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var battle = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedAvatar)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile).ThenInclude(pr => pr.EquippedFrame)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.BattleId);

        if (battle.HostUserId != userId)
        {
            throw new ForbiddenException("Yalnız otağın sahibi döyüşü başlada bilər.");
        }

        if (battle.Mode != BattleMode.Room || battle.Status != BattleStatus.Waiting)
        {
            throw new ConflictException("Bu döyüş başladıla bilməz.");
        }

        if (battle.Participants.Count < 2)
        {
            throw new ConflictException("Döyüşü başlatmaq üçün ən azı 2 iştirakçı lazımdır.");
        }

        battle.Status = BattleStatus.InProgress;
        battle.StartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var dto = BattleMapper.ToDto(battle);
        await _battleHubNotifier.NotifyBattleUpdated(battle.Id, dto, cancellationToken);

        return dto;
    }
}
