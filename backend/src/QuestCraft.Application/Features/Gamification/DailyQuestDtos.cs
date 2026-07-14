namespace QuestCraft.Application.Features.Gamification;

public record DailyQuestDto(
    int Id,
    string Title,
    string? Description,
    int CurrentProgress,
    int TargetValue,
    bool IsCompleted,
    bool RewardClaimed,
    int XpReward,
    int CoinReward);

public record ClaimDailyQuestResultDto(DailyQuestDto Quest, int TotalXp, int TotalCoins, int Level, List<string> NewAchievements);
