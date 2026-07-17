using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ChatMessage : BaseEntity
{
    public string Content { get; set; } = default!;
    public bool IsRead { get; set; }

    public int SenderId { get; set; }
    public User Sender { get; set; } = default!;

    public int RecipientId { get; set; }
    public User Recipient { get; set; } = default!;
}
