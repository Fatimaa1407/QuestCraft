using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class TestCase : BaseEntity
{
    public string Input { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;
    public int OrderIndex { get; set; }

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;
}
