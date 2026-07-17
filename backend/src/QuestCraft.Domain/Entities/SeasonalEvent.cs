using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class SeasonalEvent : BaseEntity
{
    public string Name { get; set; } = default!;
    public string? NameEn { get; set; }
    public string? Description { get; set; }
    public string? DescriptionEn { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Emoji { get; set; }
}
