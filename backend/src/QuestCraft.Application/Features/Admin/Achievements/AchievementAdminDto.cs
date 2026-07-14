namespace QuestCraft.Application.Features.Admin.Achievements;

public record AchievementAdminDto(
    int Id,
    string Name,
    string? NameEn,
    string Description,
    string? DescriptionEn,
    string? IconUrl,
    string ConditionType,
    int ConditionValue,
    int XpReward,
    int CoinReward,
    bool IsActive);
