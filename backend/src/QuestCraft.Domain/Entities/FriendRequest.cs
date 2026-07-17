using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

// A single row represents both the request AND, once Accepted, the friendship itself — there's no
// separate "Friendship" table. Querying "are A and B friends" means looking for an Accepted row in
// either direction (RequesterId/AddresseeId can appear on either side).
public class FriendRequest : BaseEntity
{
    public int RequesterId { get; set; }
    public User Requester { get; set; } = default!;

    public int AddresseeId { get; set; }
    public User Addressee { get; set; } = default!;

    public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    public DateTime? RespondedAt { get; set; }
}
