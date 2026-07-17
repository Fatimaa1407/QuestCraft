using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record JoinBattleCommand(int BattleId) : ICommand<BattleDto>;

public class JoinBattleCommandHandler : IRequestHandler<JoinBattleCommand, BattleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;
    private readonly IBattleHubNotifier _battleHubNotifier;

    public JoinBattleCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier, IBattleHubNotifier battleHubNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
        _battleHubNotifier = battleHubNotifier;
    }

    public async Task<BattleDto> Handle(JoinBattleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var battle = await _context.Battles
            .Include(b => b.Challenge).ThenInclude(c => c.TestCases)
            .Include(b => b.Challenge).ThenInclude(c => c.HiddenTestCases)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(b => b.Id == request.BattleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Battle), request.BattleId);

        if (battle.Status != BattleStatus.Waiting)
        {
            throw new ConflictException("Bu döyüşə artıq qoşula bilməzsiniz.");
        }

        if (battle.Participants.Any(p => p.UserId == userId))
        {
            throw new ConflictException("Siz artıq bu döyüşdəsiniz.");
        }

        if (battle.Mode == BattleMode.Duel && battle.InvitedUserId != userId)
        {
            throw new ForbiddenException("Bu duel dəvəti sizə göndərilməyib.");
        }

        if (battle.Participants.Count >= battle.MaxPlayers)
        {
            throw new ConflictException("Döyüş otağı doludur.");
        }

        var totalTestCases = battle.Challenge.TestCases.Count + battle.Challenge.HiddenTestCases.Count;
        // Adding to the DbSet is enough — EF's relationship fixup already links it into
        // battle.Participants via the BattleId FK (both entities are tracked), so also calling
        // battle.Participants.Add(participant) here would double it up in this in-memory collection.
        var participant = new BattleParticipant { BattleId = battle.Id, UserId = userId, TotalTestCases = totalTestCases };
        _context.BattleParticipants.Add(participant);

        // Duels start the instant the invited friend joins — a Room waits for the host to start it
        // manually so more players can trickle in first.
        if (battle.Mode == BattleMode.Duel)
        {
            battle.Status = BattleStatus.InProgress;
            battle.StartedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var user = await _context.Users.Include(u => u.Profile).FirstAsync(u => u.Id == userId, cancellationToken);
        participant.User = user;

        var dto = BattleMapper.ToDto(battle);
        await _battleHubNotifier.NotifyBattleUpdated(battle.Id, dto, cancellationToken);

        return dto;
    }
}
