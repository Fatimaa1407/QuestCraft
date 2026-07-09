using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class QuestionOption : BaseEntity
{
    public string Text { get; set; } = default!;
    public bool IsCorrect { get; set; }

    public int QuestionId { get; set; }
    public Question Question { get; set; } = default!;
}
