using System.Security.Cryptography;
using System.Text;

namespace QuestCraft.Application.Common;

// Deterministic rather than random — re-downloading the same person's certificate later always
// produces the same ID, which is what a "certificate ID" needs to mean for it to ever be useful
// for verification. No persisted column: derived from the two facts that already uniquely and
// permanently identify this completion (who, and the one-time moment they finished), so the
// verify lookup can recompute and match it against any completed user without storing it anywhere.
public static class CertificateIdGenerator
{
    public static string Generate(int userId, DateTime gameCompletedAt)
    {
        var seed = $"{userId}-{gameCompletedAt:O}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return $"QC-{Convert.ToHexString(hash)[..8]}";
    }
}
