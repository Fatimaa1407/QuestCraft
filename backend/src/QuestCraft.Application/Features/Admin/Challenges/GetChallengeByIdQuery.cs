using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record GetChallengeByIdQuery(int Id) : IQuery<ChallengeDetailDto>;

public class GetChallengeByIdQueryHandler : IRequestHandler<GetChallengeByIdQuery, ChallengeDetailDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChallengeByIdQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<ChallengeDetailDto> Handle(GetChallengeByIdQuery request, CancellationToken cancellationToken)
    {
        var isAdmin = _currentUser.Role == "Admin";

        var challenge = await _context.Challenges
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .Include(c => c.TestCases)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Challenge), request.Id);

        // Unpublished challenges don't exist as far as students are concerned.
        if (!challenge.IsPublished && !isAdmin)
        {
            throw new NotFoundException(nameof(Challenge), request.Id);
        }

        List<HiddenTestCaseDto>? hiddenTestCases = null;
        if (isAdmin)
        {
            hiddenTestCases = await _context.HiddenTestCases
                .Where(h => h.ChallengeId == request.Id)
                .OrderBy(h => h.OrderIndex)
                .Select(h => new HiddenTestCaseDto(h.Id, h.Input, h.ExpectedOutput, h.OrderIndex, h.Weight))
                .ToListAsync(cancellationToken);
        }

        var hasHint = !string.IsNullOrWhiteSpace(challenge.Hint);
        var isHintUnlocked = isAdmin || (hasHint && _currentUser.UserId is not null &&
            await _context.ChallengeHints.AnyAsync(h => h.UserId == _currentUser.UserId && h.ChallengeId == request.Id, cancellationToken));

        return new ChallengeDetailDto(
            challenge.Id,
            challenge.Title,
            challenge.Description,
            challenge.CategoryId,
            challenge.Category.Name,
            challenge.DifficultyId,
            challenge.Difficulty.Name,
            challenge.TimeLimitMs,
            challenge.MemoryLimitMb,
            challenge.XpReward,
            challenge.CoinReward,
            challenge.StarterCode,
            challenge.Constraints,
            challenge.InputFormat,
            challenge.OutputFormat,
            challenge.SampleInput,
            challenge.SampleOutput,
            isHintUnlocked ? challenge.Hint : null,
            hasHint,
            isHintUnlocked,
            challenge.IsPublished,
            challenge.TestCases.OrderBy(t => t.OrderIndex)
                .Select(t => new TestCaseDto(t.Id, t.Input, t.ExpectedOutput, t.OrderIndex))
                .ToList(),
            hiddenTestCases);
    }
}
