using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Gamification;

public record CurrentSeasonalEventDto(int Id, string Name, string? Description, string? Emoji, DateOnly EndDate);

public record GetCurrentSeasonalEventQuery : IQuery<CurrentSeasonalEventDto?>;

public class GetCurrentSeasonalEventQueryHandler : IRequestHandler<GetCurrentSeasonalEventQuery, CurrentSeasonalEventDto?>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetCurrentSeasonalEventQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<CurrentSeasonalEventDto?> Handle(GetCurrentSeasonalEventQuery request, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var isEnglish = _currentUser.IsEnglish;

        var current = await _context.SeasonalEvents
            .Where(e => e.IsActive && e.StartDate <= today && e.EndDate >= today)
            .OrderBy(e => e.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (current is null)
        {
            return null;
        }

        return new CurrentSeasonalEventDto(
            current.Id,
            LocalizationHelper.Pick(current.Name, current.NameEn, isEnglish),
            LocalizationHelper.PickNullable(current.Description, current.DescriptionEn, isEnglish),
            current.Emoji,
            current.EndDate);
    }
}
