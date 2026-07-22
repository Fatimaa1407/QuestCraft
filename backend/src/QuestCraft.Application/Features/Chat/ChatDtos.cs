namespace QuestCraft.Application.Features.Chat;

public record ChatMessageDto(int Id, int SenderId, int RecipientId, string Content, DateTime CreatedAt, bool IsRead);

public record ConversationDto(int FriendUserId, string FriendUsername, string? FriendAvatarUrl, string? LastMessage, DateTime? LastMessageAt, int UnreadCount, string? FriendFrameImageUrl);
