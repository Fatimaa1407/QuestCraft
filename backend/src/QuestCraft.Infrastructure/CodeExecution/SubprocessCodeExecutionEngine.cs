using System.Diagnostics;
using System.Text;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Infrastructure.CodeExecution;

/// <summary>
/// Executes untrusted C# submissions by compiling them into a throwaway console project and running
/// each test case as its own OS process (no Docker required). Time limit is enforced via Process.Kill,
/// memory limit via periodic WorkingSet64 polling. This is a best-effort sandbox, not a hard security
/// boundary — see docs/ARCHITECTURE.md §13 for the reasoning and the Docker/Judge0 upgrade path.
/// </summary>
public class SubprocessCodeExecutionEngine : ICodeExecutionEngine
{
    // Best-effort deny-list: catches obviously dangerous APIs without pretending to be a full sandbox.
    private static readonly string[] BannedTokens =
    [
        "System.IO.File", "System.IO.Directory", "System.IO.FileStream",
        "System.Diagnostics.Process", "System.Net.", "System.Net.Http",
        "Environment.Exit", "Environment.GetEnvironmentVariable", "DllImport",
        "unsafe ", "System.Reflection.Emit", "AppDomain", "System.Runtime.InteropServices",
    ];

    public async Task<CodeExecutionResult> ExecuteAsync(
        string sourceCode,
        IReadOnlyList<TestCaseInput> testCases,
        int timeLimitMs,
        int memoryLimitMb,
        CancellationToken cancellationToken)
    {
        var bannedToken = BannedTokens.FirstOrDefault(sourceCode.Contains);
        if (bannedToken is not null)
        {
            return new CodeExecutionResult(
                SubmissionVerdict.CompileError, 0, 0,
                $"Qadağan edilmiş API istifadə olunub: \"{bannedToken}\". Bu platforma yalnız stdin/stdout ilə işləyən sadə konsol proqramlarına icazə verir.",
                []);
        }

        var workDir = Path.Combine(Path.GetTempPath(), "questcraft-exec", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(workDir);

        try
        {
            await File.WriteAllTextAsync(Path.Combine(workDir, "Solution.csproj"), CsprojTemplate, cancellationToken);
            // A BOM forces the C# compiler to recognize the source file as UTF-8 rather than falling back
            // to the system code page, which otherwise mangles non-ASCII output (e.g. Azerbaijani ş/ı/ə).
            await File.WriteAllTextAsync(Path.Combine(workDir, "Program.cs"), sourceCode, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), cancellationToken);
            // Console.OutputEncoding defaults to the legacy OEM code page even when stdout is redirected,
            // which silently transliterates non-ASCII output (ş→s, ı→i). A module initializer runs before
            // the submission's own Main, so this fixes it without touching user-submitted source.
            await File.WriteAllTextAsync(Path.Combine(workDir, "EncodingBootstrap.cs"), EncodingBootstrapSource, cancellationToken);

            var outDir = Path.Combine(workDir, "out");
            var buildResult = await RunProcessAsync(
                "dotnet",
                $"build \"{Path.Combine(workDir, "Solution.csproj")}\" -c Release -o \"{outDir}\" --nologo -v quiet",
                workDir, stdin: null, timeLimitMs: 60_000, memoryLimitMb: 1024, cancellationToken);

            if (buildResult.ExitCode != 0)
            {
                var message = string.IsNullOrWhiteSpace(buildResult.Stdout) ? buildResult.Stderr : buildResult.Stdout;
                return new CodeExecutionResult(SubmissionVerdict.CompileError, 0, 0, Truncate(message, 4000), []);
            }

            var dllPath = Path.Combine(outDir, "Solution.dll");
            var results = new List<TestCaseExecutionResult>();
            var maxExecutionTimeMs = 0;
            var maxMemoryKb = 0;

            foreach (var testCase in testCases)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var run = await RunProcessAsync("dotnet", $"\"{dllPath}\"", outDir, testCase.Input, timeLimitMs, memoryLimitMb, cancellationToken);
                maxExecutionTimeMs = Math.Max(maxExecutionTimeMs, run.ElapsedMs);
                maxMemoryKb = Math.Max(maxMemoryKb, run.PeakMemoryKb);

                if (run.TimedOut)
                {
                    results.Add(new TestCaseExecutionResult(testCase.TestCaseId, testCase.IsHidden, false, "", run.ElapsedMs, SubmissionVerdict.TimeLimitExceeded));
                    continue;
                }

                if (run.MemoryExceeded || run.ExitCode != 0)
                {
                    results.Add(new TestCaseExecutionResult(testCase.TestCaseId, testCase.IsHidden, false, Truncate(run.Stderr, 1000), run.ElapsedMs, SubmissionVerdict.RuntimeError));
                    continue;
                }

                var passed = Normalize(run.Stdout) == Normalize(testCase.ExpectedOutput);
                results.Add(new TestCaseExecutionResult(testCase.TestCaseId, testCase.IsHidden, passed, Truncate(run.Stdout, 2000), run.ElapsedMs, SubmissionVerdict.WrongAnswer));
            }

            var verdict = DetermineVerdict(testCases, results);
            return new CodeExecutionResult(verdict, maxExecutionTimeMs, maxMemoryKb, null, results);
        }
        finally
        {
            TryDeleteDirectory(workDir);
        }
    }

    private static SubmissionVerdict DetermineVerdict(IReadOnlyList<TestCaseInput> testCases, List<TestCaseExecutionResult> results)
    {
        if (results.Count == 0)
        {
            return testCases.Count == 0 ? SubmissionVerdict.Accepted : SubmissionVerdict.RuntimeError;
        }

        if (results.All(r => r.Passed))
        {
            return SubmissionVerdict.Accepted;
        }

        // First failure encountered (in test order) decides the reported verdict, mirroring typical online judges.
        return results.First(r => !r.Passed).FailureReason;
    }

    private static string Normalize(string text) =>
        text.Replace("\r\n", "\n").Trim();

    private static string Truncate(string text, int maxLength) =>
        text.Length <= maxLength ? text : text[..maxLength] + "... (kəsildi)";

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch
        {
            // Best-effort cleanup — a leftover temp folder isn't worth failing the submission over.
        }
    }

    private const string CsprojTemplate = """
        <Project Sdk="Microsoft.NET.Sdk">
          <PropertyGroup>
            <OutputType>Exe</OutputType>
            <TargetFramework>net10.0</TargetFramework>
            <Nullable>enable</Nullable>
            <ImplicitUsings>enable</ImplicitUsings>
            <AssemblyName>Solution</AssemblyName>
            <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
            <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
            <InvariantGlobalization>true</InvariantGlobalization>
          </PropertyGroup>
          <ItemGroup>
            <Compile Include="Program.cs" />
            <Compile Include="EncodingBootstrap.cs" />
          </ItemGroup>
        </Project>
        """;

    private const string EncodingBootstrapSource = """
        using System.Runtime.CompilerServices;
        using System.Text;

        internal static class EncodingBootstrap
        {
            [ModuleInitializer]
            internal static void Init()
            {
                var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
                System.Console.OutputEncoding = utf8;
                try
                {
                    System.Console.InputEncoding = utf8;
                }
                catch (System.IO.IOException)
                {
                    // Input may already be fully consumed or unavailable when redirected — safe to ignore.
                }
            }
        }
        """;

    private record ProcessRunResult(int ExitCode, string Stdout, string Stderr, bool TimedOut, bool MemoryExceeded, int ElapsedMs, int PeakMemoryKb);

    private static async Task<ProcessRunResult> RunProcessAsync(
        string fileName, string arguments, string workingDirectory, string? stdin,
        int timeLimitMs, int memoryLimitMb, CancellationToken cancellationToken)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardInputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };
        process.StartInfo.EnvironmentVariables["DOTNET_NOLOGO"] = "1";
        process.StartInfo.EnvironmentVariables["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        var stopwatch = Stopwatch.StartNew();
        process.Start();

        if (stdin is not null)
        {
            await process.StandardInput.WriteAsync(stdin);
        }
        process.StandardInput.Close();

        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        var memoryExceeded = false;
        var peakMemoryKb = 0;
        using var memoryMonitorCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var memoryMonitorTask = Task.Run(async () =>
        {
            try
            {
                while (!memoryMonitorCts.IsCancellationRequested)
                {
                    await Task.Delay(50, memoryMonitorCts.Token);
                    process.Refresh();
                    if (process.HasExited)
                    {
                        break;
                    }

                    var currentKb = (int)(process.WorkingSet64 / 1024);
                    peakMemoryKb = Math.Max(peakMemoryKb, currentKb);

                    if (currentKb > memoryLimitMb * 1024)
                    {
                        memoryExceeded = true;
                        TryKill(process);
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, memoryMonitorCts.Token);

        var exitTask = process.WaitForExitAsync(cancellationToken);
        var timeoutTask = Task.Delay(timeLimitMs, cancellationToken);
        var firstCompleted = await Task.WhenAny(exitTask, timeoutTask);

        var timedOut = firstCompleted == timeoutTask && !process.HasExited;
        if (timedOut)
        {
            TryKill(process);
        }

        await process.WaitForExitAsync(CancellationToken.None);
        await memoryMonitorCts.CancelAsync();
        stopwatch.Stop();

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        return new ProcessRunResult(
            timedOut ? -1 : process.ExitCode,
            stdout, stderr, timedOut, memoryExceeded, (int)stopwatch.ElapsedMilliseconds, peakMemoryKb);
    }

    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Process may have exited between the check and the kill — safe to ignore.
        }
    }
}
