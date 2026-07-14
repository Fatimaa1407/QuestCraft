using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class DailyQuestTemplate : BaseEntity
{
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? TitleEn { get; set; }
    public string? DescriptionEn { get; set; }
    public DailyQuestTargetType TargetType { get; set; }
    public int TargetValue { get; set; }
    public int XpReward { get; set; }
    public int CoinReward { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<UserDailyQuest> Instances { get; set; } = new List<UserDailyQuest>();
}
