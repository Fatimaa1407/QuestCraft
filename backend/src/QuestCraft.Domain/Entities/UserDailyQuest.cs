using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class UserDailyQuest : BaseEntity
{
    public DateOnly QuestDate { get; set; }
    public int CurrentProgress { get; set; }
    public int TargetValue { get; set; }
    public bool IsCompleted { get; set; }
    public bool RewardClaimed { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int DailyQuestTemplateId { get; set; }
    public DailyQuestTemplate DailyQuestTemplate { get; set; } = default!;
}
