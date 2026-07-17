using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record GetChallengesQuery(
    int? CategoryId,
    int? DifficultyId,
    string? Search,
    int Page,
    int PageSize,
    bool IncludeUnpublished) : IQuery<PagedResult<ChallengeListItemDto>>;

public class GetChallengesQueryHandler : IRequestHandler<GetChallengesQuery, PagedResult<ChallengeListItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetChallengesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ChallengeListItemDto>> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
    {
        var isEnglish = _currentUser.IsEnglish;
        var isAdmin = _currentUser.Role == "Admin";
        var userLevel = 1;
        if (!isAdmin && _currentUser.UserId is not null)
        {
            userLevel = await _context.UserProfiles
                .Where(p => p.UserId == _currentUser.UserId)
                .Select(p => p.Level)
                .FirstOrDefaultAsync(cancellationToken);
            if (userLevel == 0)
            {
                userLevel = 1;
            }
        }

        // Battle-pool challenges are a separate set entirely — never shown in the leveled
        // practice list (student or admin), only ever surfaced via GetBattlePoolChallengesQuery
        // and picked at random when a Battle room/duel is created.
        var query = _context.Challenges
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .Where(c => !c.IsBattleOnly);

        if (!request.IncludeUnpublished)
        {
            query = query.Where(c => c.IsPublished);
        }

        if (request.CategoryId is not null)
        {
            query = query.Where(c => c.CategoryId == request.CategoryId);
        }

        if (request.DifficultyId is not null)
        {
            query = query.Where(c => c.DifficultyId == request.DifficultyId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = query.Where(c => c.Title.Contains(request.Search) || (c.Tags != null && c.Tags.Contains(request.Search)));
        }

        var projected = query
            .OrderBy(c => c.RequiredLevel)
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new ChallengeListItemDto(
                c.Id,
                isEnglish && c.TitleEn != null && c.TitleEn != "" ? c.TitleEn : c.Title,
                c.Category.Name, c.Difficulty.Name, c.XpReward, c.CoinReward, c.IsPublished,
                c.RequiredLevel, !isAdmin && c.RequiredLevel > userLevel, c.Tags));

        return await PagedResult<ChallengeListItemDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
