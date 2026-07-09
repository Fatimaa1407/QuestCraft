using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ChallengeCategory : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    public ICollection<Challenge> Challenges { get; set; } = new List<Challenge>();
    public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();
}
