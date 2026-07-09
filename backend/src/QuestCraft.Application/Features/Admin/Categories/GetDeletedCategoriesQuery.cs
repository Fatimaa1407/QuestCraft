using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Categories;

public record GetDeletedCategoriesQuery : IQuery<List<CategoryDto>>;

public class GetDeletedCategoriesQueryHandler : IRequestHandler<GetDeletedCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<CategoryDto>> Handle(GetDeletedCategoriesQuery request, CancellationToken cancellationToken) =>
        _context.ChallengeCategories
            .IgnoreQueryFilters()
            .Where(c => c.IsDeleted)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl))
            .ToListAsync(cancellationToken);
}
