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
}
