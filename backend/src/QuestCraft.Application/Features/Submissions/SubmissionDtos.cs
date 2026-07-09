namespace QuestCraft.Application.Features.Submissions;

public record RunTestResultDto(bool Passed, string Input, string ExpectedOutput, string ActualOutput, int ExecutionTimeMs);

public record RunResultDto(string Verdict, int ExecutionTimeMs, string? CompileErrorMessage, List<RunTestResultDto> Results);

// Hidden test cases never expose Input/ExpectedOutput/ActualOutput to the caller — only pass/fail.
public record SubmissionTestResultDto(bool IsHidden, bool Passed, string? Input, string? ExpectedOutput, string? ActualOutput);

public record SubmissionResultDto(
    int SubmissionId,
    string Verdict,
    int PassedTestCases,
    int TotalTestCases,
    int ExecutionTimeMs,
    int MemoryUsedKb,
    int XpEarned,
    int CoinEarned,
    string? CompileErrorMessage,
    List<SubmissionTestResultDto> Results,
    List<string> NewAchievements);

public record SubmissionListItemDto(int Id, int ChallengeId, string ChallengeTitle, string Verdict, DateTime SubmittedAt, int ExecutionTimeMs);
