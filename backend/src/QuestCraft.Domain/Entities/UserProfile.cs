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
}
