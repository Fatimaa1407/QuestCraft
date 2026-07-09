using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Common.Interfaces;

public record TestCaseInput(int TestCaseId, string Input, string ExpectedOutput, bool IsHidden);

public record TestCaseExecutionResult(
    int TestCaseId,
    bool IsHidden,
    bool Passed,
    string ActualOutput,
    int ExecutionTimeMs,
    SubmissionVerdict FailureReason = SubmissionVerdict.WrongAnswer);

public record CodeExecutionResult(
    SubmissionVerdict Verdict,
    int ExecutionTimeMs,
    int MemoryUsedKb,
    string? CompileErrorMessage,
    List<TestCaseExecutionResult> TestResults);

/// <summary>
/// Compiles and runs untrusted student C# code against a set of stdin/stdout test cases.
/// Implementations are swappable (subprocess today, Docker/Judge0 later) — callers only depend on this interface.
/// </summary>
public interface ICodeExecutionEngine
{
    Task<CodeExecutionResult> ExecuteAsync(
        string sourceCode,
        IReadOnlyList<TestCaseInput> testCases,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken);
}
