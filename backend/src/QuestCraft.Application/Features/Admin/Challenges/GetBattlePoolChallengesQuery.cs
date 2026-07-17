using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Challenges;

public record GetBattlePoolChallengesQuery : IQuery<List<ChallengeListItemDto>>;

public class GetBattlePoolChallengesQueryHandler : IRequestHandler<GetBattlePoolChallengesQuery, List<ChallengeListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBattlePoolChallengesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<ChallengeListItemDto>> Handle(GetBattlePoolChallengesQuery request, CancellationToken cancellationToken) =>
        _context.Challenges
            .Where(c => c.IsBattleOnly)
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new ChallengeListItemDto(
                c.Id, c.Title, c.Category.Name, c.Difficulty.Name, c.XpReward, c.CoinReward, c.IsPublished, c.RequiredLevel, false, c.Tags, true))
            .ToListAsync(cancellationToken);
}
