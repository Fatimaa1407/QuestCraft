namespace QuestCraft.Application.Features.Notifications;

public record NotificationDto(int Id, string Type, string Title, string Message, bool IsRead, DateTime CreatedAt);
