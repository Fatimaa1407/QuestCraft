using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class QuizAttempt : BaseEntity
{
    public int Score { get; set; }
    public int TotalQuestions { get; set; }
    public int XpEarned { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = default!;

    public ICollection<QuizAttemptAnswer> Answers { get; set; } = new List<QuizAttemptAnswer>();
}
