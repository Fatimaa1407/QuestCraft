using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Quiz : BaseEntity
{
    public string Title { get; set; } = default!;
    public int XpReward { get; set; }
    public bool IsPublished { get; set; }

    public int? CategoryId { get; set; }
    public ChallengeCategory? Category { get; set; }

    public ICollection<Question> Questions { get; set; } = new List<Question>();
    public ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
}
