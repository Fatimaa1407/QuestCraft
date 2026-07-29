namespace QuestCraft.Application.Features.Friends;

public record FriendDto(
    int UserId, string Username, string? AvatarUrl, int Level, int Xp, string? FrameImageUrl,
    string? TitleText = null, string? BadgeImageUrl = null, string? BadgeName = null);

public record FriendRequestDto(
    int Id, int RequesterId, string RequesterUsername, string? RequesterAvatarUrl, int RequesterLevel, DateTime CreatedAt, string? RequesterFrameImageUrl,
    string? RequesterTitleText = null, string? RequesterBadgeImageUrl = null, string? RequesterBadgeName = null);

// FriendStatus: "None" | "PendingSent" | "PendingReceived" | "Friends" | "Declined" | "Self"
public record UserSearchResultDto(
    int UserId, string Username, string? AvatarUrl, int Level, string FriendStatus, string? FrameImageUrl,
    string? TitleText = null, string? BadgeImageUrl = null, string? BadgeName = null);
