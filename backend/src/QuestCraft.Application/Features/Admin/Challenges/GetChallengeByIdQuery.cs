using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;

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

        if (!isAdmin && challenge.RequiredLevel > 1)
        {
            var userLevel = _currentUser.UserId is null
                ? 1
                : await _context.UserProfiles
                    .Where(p => p.UserId == _currentUser.UserId)
                    .Select(p => p.Level)
                    .FirstOrDefaultAsync(cancellationToken);

            if (userLevel < challenge.RequiredLevel)
            {
                throw new ForbiddenException($"Bu challenge üçün Level {challenge.RequiredLevel} lazımdır.");
            }
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

        var isAlreadySolved = _currentUser.UserId is not null &&
            await _context.ChallengeSubmissions.AnyAsync(
                s => s.UserId == _currentUser.UserId && s.ChallengeId == request.Id && s.Verdict == SubmissionVerdict.Accepted,
                cancellationToken);

        var isEnglish = _currentUser.IsEnglish;
        var localizedHint = isHintUnlocked ? LocalizationHelper.PickNullable(challenge.Hint, challenge.HintEn, isEnglish) : null;

        return new ChallengeDetailDto(
            challenge.Id,
            LocalizationHelper.Pick(challenge.Title, challenge.TitleEn, isEnglish),
            LocalizationHelper.Pick(challenge.Description, challenge.DescriptionEn, isEnglish),
            challenge.CategoryId,
            challenge.Category.Name,
            challenge.DifficultyId,
            challenge.Difficulty.Name,
            challenge.TimeLimitMs,
            challenge.MemoryLimitMb,
            challenge.XpReward,
            challenge.CoinReward,
            LocalizationHelper.Pick(challenge.StarterCode, challenge.StarterCodeEn, isEnglish),
            LocalizationHelper.PickNullable(challenge.Constraints, challenge.ConstraintsEn, isEnglish),
            LocalizationHelper.PickNullable(challenge.InputFormat, challenge.InputFormatEn, isEnglish),
            LocalizationHelper.PickNullable(challenge.OutputFormat, challenge.OutputFormatEn, isEnglish),
            challenge.SampleInput,
            challenge.SampleOutput,
            localizedHint,
            hasHint,
            isHintUnlocked,
            challenge.IsPublished,
            challenge.RequiredLevel,
            challenge.TestCases.OrderBy(t => t.OrderIndex)
                .Select(t => new TestCaseDto(t.Id, t.Input, t.ExpectedOutput, t.OrderIndex))
                .ToList(),
            hiddenTestCases,
            isAlreadySolved,
            isAdmin ? challenge.TitleEn : null,
            isAdmin ? challenge.DescriptionEn : null,
            isAdmin ? challenge.ConstraintsEn : null,
            isAdmin ? challenge.InputFormatEn : null,
            isAdmin ? challenge.OutputFormatEn : null,
            isAdmin ? challenge.HintEn : null,
            isAdmin ? challenge.StarterCodeEn : null);
    }
}
