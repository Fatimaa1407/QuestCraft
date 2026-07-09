using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class SystemSetting : BaseEntity
{
    public string Key { get; set; } = default!;
    public string Value { get; set; } = default!;
    public string? Description { get; set; }
}
