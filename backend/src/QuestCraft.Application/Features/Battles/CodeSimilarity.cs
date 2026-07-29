using System.Text.RegularExpressions;

namespace QuestCraft.Application.Features.Battles;

// Lightweight, dependency-free plagiarism signal for battle submissions: strips comments/whitespace,
// tokenizes, then compares overlapping token "shingles" (k-grams) with a Jaccard ratio -- the same
// basic idea as classic tools like MOSS, just without the fingerprint-hashing optimizations. This is
// a soft heuristic meant to flag likely copy-pasted solutions for an admin to review, not proof of
// cheating: renaming every identifier defeats it, but a straight copy-paste (the common case) does not.
public static class CodeSimilarity
{
    private const int ShingleSize = 8;

    private static readonly Regex LineComment = new(@"//.*$", RegexOptions.Multiline | RegexOptions.Compiled);
    private static readonly Regex BlockComment = new(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.Compiled);
    private static readonly Regex TokenPattern = new(@"[A-Za-z_][A-Za-z0-9_]*|[0-9]+(\.[0-9]+)?|[^\s]", RegexOptions.Compiled);

    public static double ComputeRatio(string codeA, string codeB)
    {
        var shinglesA = Shingle(codeA);
        var shinglesB = Shingle(codeB);
        if (shinglesA.Count == 0 || shinglesB.Count == 0)
        {
            return 0;
        }

        var intersection = shinglesA.Intersect(shinglesB).Count();
        var union = shinglesA.Union(shinglesB).Count();
        return union == 0 ? 0 : (double)intersection / union;
    }

    // Tokens are joined with a pipe separator (never produced by the tokenizer itself) before
    // hashing into the shingle set, so e.g. the boundary between "a" + "bc" can't collide with "ab" + "c".
    private static HashSet<string> Shingle(string code)
    {
        var stripped = BlockComment.Replace(code, " ");
        stripped = LineComment.Replace(stripped, " ");

        var tokens = TokenPattern.Matches(stripped).Select(m => m.Value).ToList();
        var shingles = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i + ShingleSize <= tokens.Count; i++)
        {
            shingles.Add(string.Join('|', tokens.Skip(i).Take(ShingleSize)));
        }

        return shingles;
    }
}
