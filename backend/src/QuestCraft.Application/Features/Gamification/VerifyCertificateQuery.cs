using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record VerifyCertificateQuery(string CertificateId) : IQuery<CertificateVerificationDto>;

public record CertificateVerificationDto(
    string FullName,
    int Level,
    int MaxLevel,
    int TotalXp,
    int TotalChallengesSolved,
    DateTime IssuedAt,
    string CertificateId);

public class VerifyCertificateQueryHandler : IRequestHandler<VerifyCertificateQuery, CertificateVerificationDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IContentCompletionService _completionService;

    public VerifyCertificateQueryHandler(IApplicationDbContext context, IContentCompletionService completionService)
    {
        _context = context;
        _completionService = completionService;
    }

    public async Task<CertificateVerificationDto> Handle(VerifyCertificateQuery request, CancellationToken cancellationToken)
    {
        // Certificate IDs are a one-way hash of (userId, completion moment), not a stored lookup
        // key — verifying one means recomputing that hash for every completed user and finding the
        // match. The completed-user pool is inherently small (finishing the whole game is the rare
        // case), so this scan is cheap and needs no new persisted column or index.
        var completedUsers = await _context.Users
            .Where(u => u.Profile!.GameCompletedAt != null)
            .Select(u => new { u.Id, u.FirstName, u.LastName, u.Profile!.Level, u.Profile.Xp, GameCompletedAt = u.Profile.GameCompletedAt!.Value })
            .ToListAsync(cancellationToken);

        var match = completedUsers.FirstOrDefault(
            u => CertificateIdGenerator.Generate(u.Id, u.GameCompletedAt) == request.CertificateId);

        if (match is null)
        {
            throw new NotFoundException("Certificate", request.CertificateId);
        }

        var totalSolved = await _context.ChallengeSubmissions
            .Where(s => s.UserId == match.Id && s.Verdict == SubmissionVerdict.Accepted)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var maxLevel = await _completionService.GetMaxAvailableLevelAsync(cancellationToken);

        return new CertificateVerificationDto(
            $"{match.FirstName} {match.LastName}", match.Level, maxLevel, match.Xp, totalSolved, match.GameCompletedAt, request.CertificateId);
    }
}
