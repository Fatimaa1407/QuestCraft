namespace QuestCraft.Application.Common.Interfaces;

public record CertificateData(
    string FullName,
    int Level,
    int MaxLevel,
    int TotalXp,
    int TotalChallengesSolved,
    DateTime IssuedAt,
    string CertificateId);

public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificateData data);
}
