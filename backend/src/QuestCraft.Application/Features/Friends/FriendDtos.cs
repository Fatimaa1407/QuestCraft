namespace QuestCraft.Application.Features.Friends;

public record FriendDto(int UserId, string Username, string? AvatarUrl, int Level, int Xp, string? FrameImageUrl);

public record FriendRequestDto(int Id, int RequesterId, string RequesterUsername, string? RequesterAvatarUrl, int RequesterLevel, DateTime CreatedAt, string? RequesterFrameImageUrl);

// FriendStatus: "None" | "PendingSent" | "PendingReceived" | "Friends" | "Self"
public record UserSearchResultDto(int UserId, string Username, string? AvatarUrl, int Level, string FriendStatus, string? FrameImageUrl);
