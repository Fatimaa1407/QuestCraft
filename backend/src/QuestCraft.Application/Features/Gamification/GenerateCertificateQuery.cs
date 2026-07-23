using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Gamification;

public record GenerateCertificateQuery : IQuery<byte[]>;

public class GenerateCertificateQueryHandler : IRequestHandler<GenerateCertificateQuery, byte[]>
{
    private const int RequiredLevel = 10;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICertificatePdfGenerator _pdfGenerator;

    public GenerateCertificateQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser, ICertificatePdfGenerator pdfGenerator)
    {
        _context = context;
        _currentUser = currentUser;
        _pdfGenerator = pdfGenerator;
    }

    public async Task<byte[]> Handle(GenerateCertificateQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var user = await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new { u.FirstName, u.LastName, u.Profile!.Level, u.Profile.Xp })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("User", userId);

        if (user.Level < RequiredLevel)
        {
            throw new ForbiddenException($"Sertifikat üçün {RequiredLevel}-ci səviyyəyə çatmalısınız.");
        }

        var totalSolved = await _context.ChallengeSubmissions
            .Where(s => s.UserId == userId && s.Verdict == SubmissionVerdict.Accepted)
            .Select(s => s.ChallengeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var data = new CertificateData($"{user.FirstName} {user.LastName}", user.Level, user.Xp, totalSolved, DateTime.UtcNow);
        return _pdfGenerator.Generate(data);
    }
}
