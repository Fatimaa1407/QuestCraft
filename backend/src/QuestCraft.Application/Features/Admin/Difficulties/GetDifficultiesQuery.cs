using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.Difficulties;

public record GetDifficultiesQuery : IQuery<List<DifficultyDto>>;

public class GetDifficultiesQueryHandler : IRequestHandler<GetDifficultiesQuery, List<DifficultyDto>>
{
    private readonly IApplicationDbContext _context;

    public GetDifficultiesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<DifficultyDto>> Handle(GetDifficultiesQuery request, CancellationToken cancellationToken) =>
        _context.ChallengeDifficulties
            .OrderBy(d => d.XpMultiplier)
            .Select(d => new DifficultyDto(d.Id, d.Name, d.Color, d.XpMultiplier))
            .ToListAsync(cancellationToken);
}
