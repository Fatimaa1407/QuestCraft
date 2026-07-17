using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Battles;

public record CreateDuelBattleCommand(int OpponentUserId) : ICommand<BattleDto>;

public class CreateDuelBattleCommandValidator : AbstractValidator<CreateDuelBattleCommand>
{
    public CreateDuelBattleCommandValidator()
    {
        RuleFor(x => x.OpponentUserId).GreaterThan(0);
    }
}

public class CreateDuelBattleCommandHandler : IRequestHandler<CreateDuelBattleCommand, BattleDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IRealtimeNotifier _realtimeNotifier;

    public CreateDuelBattleCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, IRealtimeNotifier realtimeNotifier)
    {
        _context = context;
        _currentUser = currentUser;
        _realtimeNotifier = realtimeNotifier;
    }

    public async Task<BattleDto> Handle(CreateDuelBattleCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        if (userId == request.OpponentUserId)
        {
            throw new ConflictException("Özünüzlə duelə çıxa bilməzsiniz.");
        }

        var areFriends = await _context.FriendRequests.AnyAsync(
            f => f.Status == FriendRequestStatus.Accepted
                && ((f.RequesterId == userId && f.AddresseeId == request.OpponentUserId)
                    || (f.RequesterId == request.OpponentUserId && f.AddresseeId == userId)),
            cancellationToken);

        if (!areFriends)
        {
            throw new ForbiddenException("Yalnız dostlarınızı duelə dəvət edə bilərsiniz.");
        }

        var challenge = await BattlePoolSelector.PickRandomAsync(_context, cancellationToken);

        var host = await _context.Users.FirstAsync(u => u.Id == userId, cancellationToken);
        var totalTestCases = challenge.TestCases.Count + challenge.HiddenTestCases.Count;

        var battle = new Battle
        {
            Mode = BattleMode.Duel,
            Status = BattleStatus.Waiting,
            MaxPlayers = 2,
            ChallengeId = challenge.Id,
            HostUserId = userId,
            InvitedUserId = request.OpponentUserId,
        };
        battle.Participants.Add(new BattleParticipant { UserId = userId, TotalTestCases = totalTestCases });

        _context.Battles.Add(battle);

        _context.Notifications.Add(new Notification
        {
            UserId = request.OpponentUserId,
            Type = NotificationType.SystemNotification,
            Title = "Duel dəvəti!",
            Message = $"{host.Username} sizi \"{challenge.Title}\" üzərində duelə dəvət etdi.",
            TitleEn = "Duel invite!",
            MessageEn = $"{host.Username} challenged you to a duel on \"{challenge.Title}\".",
        });

        await _context.SaveChangesAsync(cancellationToken);
        await _realtimeNotifier.NotifyNewNotification(request.OpponentUserId, cancellationToken);

        var saved = await _context.Battles
            .Include(b => b.Challenge)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .FirstAsync(b => b.Id == battle.Id, cancellationToken);

        return BattleMapper.ToDto(saved);
    }
}
