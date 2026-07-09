using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class SubmissionResult : BaseEntity
{
    // Points at either TestCase.Id or HiddenTestCase.Id depending on IsHidden — no FK constraint since it's polymorphic.
    public int TestCaseId { get; set; }
    public bool IsHidden { get; set; }
    public bool Passed { get; set; }
    public string? ActualOutput { get; set; }
    public int ExecutionTimeMs { get; set; }

    public int SubmissionId { get; set; }
    public ChallengeSubmission Submission { get; set; } = default!;
}
