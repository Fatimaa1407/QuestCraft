using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Categories;

public record GetCategoriesQuery : IQuery<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCategoriesQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.ChallengeCategories.Where(c => !c.IsDeleted);

        var isAdmin = _currentUser.Role == "Admin";
        if (!isAdmin)
        {
            var userLevel = 1;
            if (_currentUser.UserId is not null)
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

            // Hide categories whose entire content sits above the user's current level — otherwise
            // picking one shows nothing but a greyed-out locked section, which reads as being sent
            // into a level the user hasn't reached yet.
            query = query.Where(c =>
                c.Challenges.Any(ch => !ch.IsDeleted && ch.IsPublished && !ch.IsBattleOnly && !ch.IsDailyPuzzle && ch.RequiredLevel <= userLevel) ||
                c.Quizzes.Any(q => !q.IsDeleted && q.IsPublished && q.RequiredLevel <= userLevel));
        }

        return await query
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl))
            .ToListAsync(cancellationToken);
    }
}
