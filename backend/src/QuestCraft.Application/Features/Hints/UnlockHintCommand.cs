using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Hints;

public record UnlockHintCommand(int ChallengeId) : ICommand<string>;

public class UnlockHintCommandHandler : IRequestHandler<UnlockHintCommand, string>
{
    // Flat cost for now — a candidate for SystemSettings-driven configuration later, not worth the
    // extra indirection yet since only one hint tier exists.
    private const int HintCost = 10;

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UnlockHintCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<string> Handle(UnlockHintCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challenge = await _context.Challenges.FirstOrDefaultAsync(c => c.Id == request.ChallengeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.ChallengeId);

        if (string.IsNullOrWhiteSpace(challenge.Hint))
        {
            throw new BadRequestException("Bu challenge üçün hint mövcud deyil.");
        }

        var alreadyUnlocked = await _context.ChallengeHints
            .AnyAsync(h => h.UserId == userId && h.ChallengeId == request.ChallengeId, cancellationToken);
        if (alreadyUnlocked)
        {
            return challenge.Hint;
        }

        var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserProfile), userId);

        if (profile.Coins < HintCost)
        {
            throw new BadRequestException("Kifayət qədər coin yoxdur.");
        }

        profile.Coins -= HintCost;

        _context.ChallengeHints.Add(new ChallengeHint { UserId = userId, ChallengeId = request.ChallengeId });

        var stats = await _context.UserStatistics.FirstOrDefaultAsync(s => s.UserId == userId, cancellationToken);
        if (stats is not null)
        {
            stats.TotalCoinsSpent += HintCost;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return challenge.Hint;
    }
}
