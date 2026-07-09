using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class ChallengeSubmission : BaseEntity
{
    public string SourceCode { get; set; } = default!;
    public SubmissionVerdict Verdict { get; set; } = SubmissionVerdict.Pending;
    public int ExecutionTimeMs { get; set; }
    public int MemoryUsedKb { get; set; }
    public int XpEarned { get; set; }
    public int CoinEarned { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;

    public ICollection<SubmissionResult> Results { get; set; } = new List<SubmissionResult>();
}
