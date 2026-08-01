using QuestCraft.Infrastructure.CodeExecution;

namespace QuestCraft.UnitTests.CodeExecution;

public class SafeApiAnalyzerTests
{
    private const string ProgramWrapper = """
        using System;
        using System.Reflection;

        class Program
        {{
            static void Main()
            {{
                {0}
            }}
        }}
        """;

    private static string? Check(string bodyCode) => SafeApiAnalyzer.FindBannedApiUsage(string.Format(ProgramWrapper, bodyCode));

    [Fact]
    public void FindBannedApiUsage_DelegateMethodName_IsAllowed()
    {
        // A delegate's underlying method name is ordinary, harmless introspection (e.g. printing which
        // callback fired) — this specific case was reported as a false positive when the analyzer banned
        // all of System.Reflection wholesale.
        var result = Check("""
            Action callback = () => Console.WriteLine("hi");
            Console.WriteLine(callback.Method.Name);
            """);

        Assert.Null(result);
    }

    [Fact]
    public void FindBannedApiUsage_GetTypeName_IsAllowed()
    {
        var result = Check("""Console.WriteLine("x".GetType().FullName);""");

        Assert.Null(result);
    }

    [Fact]
    public void FindBannedApiUsage_TypeGetTypeByString_IsBanned()
    {
        // The classic reflection-by-string-name escape hatch — must stay blocked.
        var result = Check("""var t = Type.GetType("System.IO.File");""");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindBannedApiUsage_AssemblyLoad_IsBanned()
    {
        var result = Check("""var a = Assembly.Load("System");""");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindBannedApiUsage_MethodInvoke_IsBanned()
    {
        var result = Check("""
            var method = typeof(Console).GetMethod("WriteLine", Type.EmptyTypes);
            method?.Invoke(null, null);
            """);

        Assert.NotNull(result);
    }

    [Fact]
    public void FindBannedApiUsage_FileIo_IsBanned()
    {
        var result = Check("""System.IO.File.Delete("x.txt");""");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindBannedApiUsage_EnvironmentExit_IsBanned()
    {
        var result = Check("Environment.Exit(0);");

        Assert.NotNull(result);
    }

    [Fact]
    public void FindBannedApiUsage_PlainConsoleProgram_IsAllowed()
    {
        var result = Check("""
            var line = Console.ReadLine();
            Console.WriteLine(line);
            """);

        Assert.Null(result);
    }
}
