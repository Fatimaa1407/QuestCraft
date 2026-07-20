namespace QuestCraft.Application.Features.Profile;

public record MyProfileDto(string? Bio, string? AvatarUrl);

public record EquippedCosmeticsDto(
    string? AvatarUrl,
    string? FrameImageUrl,
    string? BannerImageUrl,
    string? TitleText,
    string? BadgeImageUrl,
    string? BadgeName,
    int? ThemeItemId,
    string? ThemeName);
