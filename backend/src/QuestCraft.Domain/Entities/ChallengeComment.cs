using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class ChallengeComment : BaseEntity
{
    public string Content { get; set; } = default!;
    public bool IsSpoiler { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;

    // Null = top-level comment. Set = a reply, and only one level deep — enforced in the command
    // validator, not here, since a reply's Parent is itself always guaranteed top-level by that check.
    public int? ParentCommentId { get; set; }
    public ChallengeComment? Parent { get; set; }
    public ICollection<ChallengeComment> Replies { get; set; } = new List<ChallengeComment>();
}
