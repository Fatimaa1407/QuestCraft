using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record GetDeletedDifficultiesQuery : IQuery<List<DifficultyDto>>;

public class GetDeletedDifficultiesQueryHandler : IRequestHandler<GetDeletedDifficultiesQuery, List<DifficultyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDeletedDifficultiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<DifficultyDto>> Handle(GetDeletedDifficultiesQuery request, CancellationToken cancellationToken) =>
        _context.ChallengeDifficulties
            .IgnoreQueryFilters()
            .Where(d => d.IsDeleted)
            .OrderBy(d => d.Name)
            .Select(d => new DifficultyDto(d.Id, d.Name, d.Color, d.XpMultiplier))
            .ToListAsync(cancellationToken);
}
