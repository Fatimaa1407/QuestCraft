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

    // Client-reported wall-clock time from opening the challenge to hitting Submit — client-controlled
    // like any quiz timer, so treated as a soft signal for the SpeedSolve achievement only, never for
    // scoring/rewards. Null for submissions made before this field existed or without a client timer.
    public int? SolveTimeMs { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;

    public ICollection<SubmissionResult> Results { get; set; } = new List<SubmissionResult>();
}
