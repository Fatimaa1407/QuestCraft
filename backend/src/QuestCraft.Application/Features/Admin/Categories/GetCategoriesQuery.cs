using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Categories;

public record GetCategoriesQuery : IQuery<List<CategoryDto>>;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, List<CategoryDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoriesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<CategoryDto>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken) =>
        _context.ChallengeCategories
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description, c.IconUrl))
            .ToListAsync(cancellationToken);
}
