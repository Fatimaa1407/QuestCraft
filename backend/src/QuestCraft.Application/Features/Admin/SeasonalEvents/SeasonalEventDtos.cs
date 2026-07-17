namespace QuestCraft.Application.Features.Admin.SeasonalEvents;

public record SeasonalEventDto(
    int Id,
    string Name,
    string? NameEn,
    string? Description,
    string? DescriptionEn,
    DateOnly StartDate,
    DateOnly EndDate,
    bool IsActive,
    string? Emoji);
