namespace QuestCraft.Application.Common.Interfaces;

public record CertificateData(string FullName, int Level, int TotalXp, int TotalChallengesSolved, DateTime IssuedAt);

public interface ICertificatePdfGenerator
{
    byte[] Generate(CertificateData data);
}
