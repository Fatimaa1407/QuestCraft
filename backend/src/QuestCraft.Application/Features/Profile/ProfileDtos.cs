namespace QuestCraft.Application.Features.Profile;

public record MyProfileDto(string? Bio, string? AvatarUrl, bool IsGameComplete);

public record PublicProfileDto(
    int UserId,
    string Username,
    int Level,
    int Xp,
    string? Bio,
    string? AvatarUrl,
    string? FrameImageUrl,
    string? BannerImageUrl,
    string? TitleText,
    string? BadgeImageUrl,
    string? BadgeName,
    DateTime JoinedAt,
    bool IsGameComplete);

public record EquippedCosmeticsDto(
    string? AvatarUrl,
    string? FrameImageUrl,
    string? BannerImageUrl,
    string? TitleText,
    string? BadgeImageUrl,
    string? BadgeName,
    int? ThemeItemId,
    string? ThemeName);
