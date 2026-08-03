namespace QuestCraft.Application.Features.Chat;

public record ChatMessageDto(int Id, int SenderId, int RecipientId, string Content, string? ImageDataUrl, DateTime CreatedAt, bool IsRead);

public record ConversationDto(
    int FriendUserId, string FriendUsername, string? FriendAvatarUrl, string? LastMessage, DateTime? LastMessageAt, int UnreadCount, string? FriendFrameImageUrl,
    string? FriendTitleText = null, string? FriendBadgeImageUrl = null, string? FriendBadgeName = null);
