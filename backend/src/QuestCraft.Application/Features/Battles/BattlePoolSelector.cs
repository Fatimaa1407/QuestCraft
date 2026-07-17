using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Battles;

public static class BattlePoolSelector
{
    // Battle challenges are drawn at random from a pool kept entirely separate from the leveled
    // practice list (Challenge.IsBattleOnly) — nobody could have already seen or solved one through
    // normal play, so every battle starts everyone on equal footing.
    public static async Task<Challenge> PickRandomAsync(IApplicationDbContext context, CancellationToken cancellationToken)
    {
        var poolIds = await context.Challenges
            .Where(c => c.IsBattleOnly && c.IsPublished)
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        if (poolIds.Count == 0)
        {
            throw new ConflictException("Battle Pool boşdur. Admin ən azı bir battle tapşırığı əlavə etməlidir.");
        }

        var pickedId = poolIds[Random.Shared.Next(poolIds.Count)];

        return await context.Challenges
            .Include(c => c.TestCases)
            .Include(c => c.HiddenTestCases)
            .FirstAsync(c => c.Id == pickedId, cancellationToken);
    }
}
