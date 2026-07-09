using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ChallengeDifficulty : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Color { get; set; }
    public double XpMultiplier { get; set; } = 1.0;

    public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
}
