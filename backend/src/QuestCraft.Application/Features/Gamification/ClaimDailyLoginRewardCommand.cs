using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Gamification;

public record DailyLoginRewardDto(bool AlreadyClaimed, int CoinsAwarded, int XpAwarded, bool WasMysteryBonus, int NewCoinsTotal, int NewXpTotal);

public record ClaimDailyLoginRewardCommand : ICommand<DailyLoginRewardDto>;

// Idempotent by design — safe to call once per app load. If today's reward was already claimed
// (by this call or an earlier one today), it just reports that instead of throwing, so the
// frontend can call it unconditionally on every session start without special-casing errors.
public class ClaimDailyLoginRewardCommandHandler : IRequestHandler<ClaimDailyLoginRewardCommand, DailyLoginRewardDto>
{
    private const int BaseCoins = 20;
    private const int BaseXp = 10;
    private const double MysteryBonusChance = 0.15;
    private const int MysteryBonusMultiplier = 3;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ClaimDailyLoginRewardCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<DailyLoginRewardDto> Handle(ClaimDailyLoginRewardCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (profile.LastLoginRewardClaimedAt == today)
        {
            return new DailyLoginRewardDto(true, 0, 0, false, profile.Coins, profile.Xp);
        }

        var isMysteryBonus = Random.Shared.NextDouble() < MysteryBonusChance;
        var coinsAwarded = isMysteryBonus ? BaseCoins * MysteryBonusMultiplier : BaseCoins;
        var xpAwarded = BaseXp;

        profile.Coins += coinsAwarded;
        profile.Xp += xpAwarded;
        profile.LastLoginRewardClaimedAt = today;
        // Passive reward, not a completion — Level intentionally stays completion-based (matches
        // ClaimDailyQuestRewardCommand's same rule) and only changes via challenge/quiz progress.

        _context.XpTransactions.Add(new XpTransaction { UserId = userId, Amount = xpAwarded, Source = "DailyLoginReward" });

        await _context.SaveChangesAsync(cancellationToken);

        return new DailyLoginRewardDto(false, coinsAwarded, xpAwarded, isMysteryBonus, profile.Coins, profile.Xp);
    }
}
