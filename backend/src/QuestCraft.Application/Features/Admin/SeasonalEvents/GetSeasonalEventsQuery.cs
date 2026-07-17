using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.SeasonalEvents;

public record GetSeasonalEventsQuery : IQuery<List<SeasonalEventDto>>;

public class GetSeasonalEventsQueryHandler : IRequestHandler<GetSeasonalEventsQuery, List<SeasonalEventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetSeasonalEventsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<List<SeasonalEventDto>> Handle(GetSeasonalEventsQuery request, CancellationToken cancellationToken) =>
        _context.SeasonalEvents
            .OrderByDescending(e => e.StartDate)
            .Select(e => new SeasonalEventDto(e.Id, e.Name, e.NameEn, e.Description, e.DescriptionEn, e.StartDate, e.EndDate, e.IsActive, e.Emoji))
            .ToListAsync(cancellationToken);
}
