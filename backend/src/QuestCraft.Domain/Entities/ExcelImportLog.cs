using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ExcelImportLog : BaseEntity
{
    public string FileName { get; set; } = default!;
    public string EntityType { get; set; } = default!;
    public int TotalRows { get; set; }
    public int SuccessRows { get; set; }
    public int FailedRows { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    public int UserId { get; set; }
    public User User { get; set; } = default!;
}
