using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace QuestCraft.Infrastructure.CodeExecution;

/// <summary>
/// Rejects a dangerous-API call before a submission is ever compiled/run, by resolving every
/// identifier through Roslyn's semantic model rather than pattern-matching the raw source text.
/// This is what makes it resistant to the easy bypasses a plain string/substring ban list falls to:
/// <c>using IO = System.IO;</c> then <c>IO.File.Delete(...)</c>, or building a banned type name via
/// string concatenation, still resolve to the same symbol the compiler would actually bind to — so
/// they're still caught. It is still a best-effort deny-list, not a capability sandbox: this alone
/// does not stop the compiled program from touching the filesystem/network/host process at *runtime*
/// via some symbol this list doesn't yet know about, or an escape this project's threat model didn't
/// anticipate. See docs/ARCHITECTURE.md §13 for the OS/container-level isolation this still needs
/// before real production exposure.
/// </summary>
internal static class SafeApiAnalyzer
{
    // Whole namespaces where essentially everything in them is dangerous. System.Reflection itself is
    // deliberately NOT banned wholesale here — ordinary, harmless code reaches into it far more often
    // than the analyzer's first version accounted for (e.g. a delegate's `.Method.Name` for a debug
    // print resolves through System.Reflection.MemberInfo). Only its genuinely dangerous surface
    // (loading/invoking/emitting) is banned below via BannedTypes/BannedMembers instead.
    private static readonly string[] BannedNamespaces =
    [
        "System.IO",
        "System.Net",
        "System.Reflection.Emit", // dynamic IL generation at runtime — always dangerous
        "System.Runtime.InteropServices",
        "System.Runtime.Loader",
        "System.Security.Cryptography.X509Certificates",
    ];

    // Specific types within otherwise-benign namespaces (e.g. System.Diagnostics also has the
    // harmless Stopwatch/Debug/Trace, so only Process itself — the process-spawning surface — is banned).
    private static readonly string[] BannedTypes =
    [
        "System.Diagnostics.Process",
        "System.Diagnostics.ProcessStartInfo",
        "System.AppDomain",
        "System.Activator",
        "System.Reflection.Assembly", // blocks Assembly.Load*/GetExecutingAssembly — loading/inspecting arbitrary code
    ];

    // Specific members on otherwise-benign types.
    private static readonly (string Type, string Member)[] BannedMembers =
    [
        ("System.Environment", "Exit"),
        ("System.Environment", "GetEnvironmentVariable"),
        ("System.Environment", "GetEnvironmentVariables"),
        ("System.Environment", "SetEnvironmentVariable"),
        // Type.GetType(string) — the reflection-by-string-name escape hatch — stays banned, but as
        // just this one static method rather than all of System.Type, so ordinary instance reads like
        // `obj.GetType().Name`/`.FullName` (declared on Type itself, not just inherited from
        // MemberInfo) aren't caught in the same net.
        ("System.Type", "GetType"),
        // Dynamic method/constructor invocation and private-member mutation — the actual reflection
        // "escape hatches" — stay banned even though the rest of System.Reflection is now readable.
        ("System.Reflection.MethodBase", "Invoke"),
        ("System.Reflection.ConstructorInfo", "Invoke"),
        ("System.Reflection.FieldInfo", "SetValue"),
        ("System.Reflection.PropertyInfo", "SetValue"),
    ];

    private static readonly string[] TrustedAssemblyPaths =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator);

    public static string? FindBannedApiUsage(string sourceCode)
    {
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();

        if (root.DescendantTokens().Any(t => t.IsKind(SyntaxKind.UnsafeKeyword)))
        {
            return "unsafe kod";
        }

        var references = TrustedAssemblyPaths.Select(p => (MetadataReference)MetadataReference.CreateFromFile(p));
        var compilation = CSharpCompilation.Create("SecurityCheck", [tree], references, new CSharpCompilationOptions(OutputKind.ConsoleApplication));
        var semanticModel = compilation.GetSemanticModel(tree);

        foreach (var node in root.DescendantNodes())
        {
            // Only these node kinds can ever resolve to a type/member reference worth checking —
            // skipping everything else keeps this from re-binding every literal/operator in the file.
            if (node is not (IdentifierNameSyntax or GenericNameSyntax or MemberAccessExpressionSyntax))
            {
                continue;
            }

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol is null)
            {
                continue;
            }

            var containingType = symbol as INamedTypeSymbol ?? symbol.ContainingType;
            if (containingType is null)
            {
                continue;
            }

            var fullTypeName = containingType.ToDisplayString();
            var containingNamespace = containingType.ContainingNamespace?.ToDisplayString() ?? "";

            if (BannedNamespaces.Any(ns => containingNamespace == ns || containingNamespace.StartsWith(ns + ".", StringComparison.Ordinal)))
            {
                return fullTypeName;
            }

            if (BannedTypes.Contains(fullTypeName))
            {
                return fullTypeName;
            }

            if (symbol is IMethodSymbol or IPropertySymbol
                && BannedMembers.Any(b => b.Type == fullTypeName && b.Member == symbol.Name))
            {
                return $"{fullTypeName}.{symbol.Name}";
            }
        }

        return null;
    }
}
