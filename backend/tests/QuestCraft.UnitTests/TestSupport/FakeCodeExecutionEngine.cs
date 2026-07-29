using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.UnitTests.TestSupport;

// Deterministic stand-in for the real subprocess-based code execution engine. Defaults to "every
// test case passes"; set AllPass = false to simulate a failing/partial submission instead.
public class FakeCodeExecutionEngine : ICodeExecutionEngine
{
    public bool AllPass { get; set; } = true;
    public int PassCount { get; set; } = int.MaxValue;

    public Task<CodeExecutionResult> ExecuteAsync(
        string sourceCode, IReadOnlyList<TestCaseInput> testCases, int timeLimitMs, int memoryLimitMb, CancellationToken cancellationToken)
    {
        var results = testCases
            .Select((t, i) => new TestCaseExecutionResult(
                t.TestCaseId, t.IsHidden, Passed: AllPass && i < PassCount, ActualOutput: AllPass && i < PassCount ? t.ExpectedOutput : "wrong", ExecutionTimeMs: 5))
            .ToList();

        var verdict = results.All(r => r.Passed) && results.Count > 0 ? SubmissionVerdict.Accepted : SubmissionVerdict.WrongAnswer;
        return Task.FromResult(new CodeExecutionResult(verdict, ExecutionTimeMs: 5, MemoryUsedKb: 1024, CompileErrorMessage: null, results));
    }
}
