using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record GetDeletedChallengesQuery : IQuery<List<ChallengeListItemDto>>;

public class GetDeletedChallengesQueryHandler : IRequestHandler<GetDeletedChallengesQuery, List<ChallengeListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedChallengesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<ChallengeListItemDto>> Handle(GetDeletedChallengesQuery request, CancellationToken cancellationToken) =>
        _context.Challenges
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted)
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .OrderByDescending(c => c.UpdatedAt)
            .Select(c => new ChallengeListItemDto(
                c.Id, c.Title, c.Category.Name, c.Difficulty.Name, c.XpReward, c.CoinReward, c.IsPublished))
            .ToListAsync(cancellationToken);
}
