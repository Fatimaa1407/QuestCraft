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

    public GetChallengesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<ChallengeListItemDto>> Handle(GetChallengesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Challenges
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .AsQueryable();

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
            query = query.Where(c => c.Title.Contains(request.Search));
        }

        var projected = query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChallengeListItemDto(
                c.Id, c.Title, c.Category.Name, c.Difficulty.Name, c.XpReward, c.CoinReward, c.IsPublished));

        return PagedResult<ChallengeListItemDto>.CreateAsync(projected, request.Page, request.PageSize, cancellationToken);
    }
}
