using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = default!;

    public ICollection<User> Users { get; set; } = new List<User>();
}
