using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Question : BaseEntity
{
    public string Text { get; set; } = default!;
    public string? Explanation { get; set; }
    public string? TextEn { get; set; }
    public string? ExplanationEn { get; set; }

    public int QuizId { get; set; }
    public Quiz Quiz { get; set; } = default!;

    public ICollection<QuestionOption> Options { get; set; } = new List<QuestionOption>();
}
