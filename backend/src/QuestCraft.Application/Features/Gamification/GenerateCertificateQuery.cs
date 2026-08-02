using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record GenerateCertificateQuery : IQuery<byte[]>;

public class GenerateCertificateQueryHandler : IRequestHandler<GenerateCertificateQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICertificatePdfGenerator _pdfGenerator;
    private readonly IContentCompletionService _completionService;

    public GenerateCertificateQueryHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, ICertificatePdfGenerator pdfGenerator, IContentCompletionService completionService)
    {
        _context = context;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
        _completionService = completionService;
    }

    public async Task<byte[]> Handle(GenerateCertificateQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName, u.Profile!.Level, u.Profile.Xp, u.Profile.GameCompletedAt })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", userId);

        // Tied to actually finishing the game (GameCompletionService's one-time flag) rather than a
        // hardcoded level number — this way it automatically tracks wherever the current max level
        // ends up (see IContentCompletionService.GetMaxAvailableLevelAsync), including if more
        // content is published later and the max level rises past 12.
        if (user.GameCompletedAt is null)
        {
            throw new ForbiddenException("Sertifikat üçün bütün QuestCraft məzmununu tamamlamalısınız.");
        }

        var totalSolved = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var maxLevel = await _completionService.GetMaxAvailableLevelAsync(cancellationToken);
        var certificateId = CertificateIdGenerator.Generate(userId, user.GameCompletedAt.Value);

        var data = new CertificateData(
            $"{user.FirstName} {user.LastName}", user.Level, maxLevel, user.Xp, totalSolved, DateTime.UtcNow, certificateId);
        return _pdfGenerator.Generate(data);
    }
}
