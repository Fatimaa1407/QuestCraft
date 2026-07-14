namespace QuestCraft.Application.Features.Admin.DailyQuestTemplates;

public record DailyQuestTemplateAdminDto(
    int Id,
    string Title,
    string? TitleEn,
    string? Description,
    string? DescriptionEn,
    string TargetType,
    int TargetValue,
    int XpReward,
    int CoinReward,
    bool IsActive);
