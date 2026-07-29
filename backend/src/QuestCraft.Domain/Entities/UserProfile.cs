using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class UserProfile : BaseEntity
{
    public int Xp { get; set; }
    public int Coins { get; set; }
    public int Level { get; set; } = 1;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int? EquippedFrameId { get; set; }
    public MarketplaceItem? EquippedFrame { get; set; }

    public int? EquippedTitleId { get; set; }
    public MarketplaceItem? EquippedTitle { get; set; }

    public int? EquippedThemeId { get; set; }
    public MarketplaceItem? EquippedTheme { get; set; }

    public int? EquippedAvatarId { get; set; }
    public MarketplaceItem? EquippedAvatar { get; set; }

    public int? EquippedBannerId { get; set; }
    public MarketplaceItem? EquippedBanner { get; set; }

    public int? EquippedBadgeId { get; set; }
    public MarketplaceItem? EquippedBadge { get; set; }

    // Null until the user's first daily-login-reward claim; compared against "today" (UTC date)
    // to decide claim eligibility, mirrors Streak.LastActivityDate's nullable-DateOnly pattern.
    public DateOnly? LastLoginRewardClaimedAt { get; set; }

    // User-configured daily targets; null means the user hasn't set that goal. Progress against these
    // is computed statelessly from today's ChallengeSubmission/XpTransaction/BattleParticipant rows.
    public int? DailyGoalChallenges { get; set; }
    public int? DailyGoalXp { get; set; }
    public int? DailyGoalBattles { get; set; }

    // Optimistic-concurrency token, auto-incremented in ApplicationDbContext.SaveChangesAsync for
    // every Modified UserProfile (not manually touched by individual handlers — easy to forget, and
    // this way nothing new added later can accidentally skip it). Every Xp/Coins-granting handler
    // (challenge/quiz solve, purchase, daily quest claim, achievement unlock, battle win, ...) reads
    // this profile then writes back to it — without a concurrency token, two simultaneous grants both
    // read the same starting balance and the second write silently clobbers the first (a real,
    // exploitable double-XP/double-spend bug several handlers had). ConcurrencyRetryBehavior catches
    // the resulting DbUpdateConcurrencyException and retries the whole command against fresh data.
    public int Version { get; set; }
}
