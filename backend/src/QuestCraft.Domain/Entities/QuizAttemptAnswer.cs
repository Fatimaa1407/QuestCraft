using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class QuizAttemptAnswer : BaseEntity
{
    public bool IsCorrect { get; set; }

    public int QuizAttemptId { get; set; }
    public QuizAttempt QuizAttempt { get; set; } = default!;

    public int QuestionId { get; set; }
    public Question Question { get; set; } = default!;

    public int? SelectedOptionId { get; set; }
    public QuestionOption? SelectedOption { get; set; }
}
