using QuestCraft.Domain.Common;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Domain.Entities;

public class Battle : BaseEntity
{
    // Short human-typeable join code for Room mode (e.g. "K7QX2P") — null for Duel battles, which
    // are joined via direct invite/accept instead of a code.
    public string? JoinCode { get; set; }

    public BattleMode Mode { get; set; }
    public BattleStatus Status { get; set; } = BattleStatus.Waiting;
    public int MaxPlayers { get; set; }

    public int ChallengeId { get; set; }
    public Challenge Challenge { get; set; } = default!;

    public int HostUserId { get; set; }
    public User HostUser { get; set; } = default!;

    // Duel mode only — the specific friend invited, so only they (not anyone who learns the battle
    // id) can accept. Null for Room mode, which is joined by code instead.
    public int? InvitedUserId { get; set; }
    public User? InvitedUser { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public ICollection<BattleParticipant> Participants { get; set; } = new List<BattleParticipant>();
}

public class BattleParticipant : BaseEntity
{
    public int BattleId { get; set; }
    public Battle Battle { get; set; } = default!;

    public int UserId { get; set; }
    public User User { get; set; } = default!;

    public bool HasFinished { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? Rank { get; set; }
    public int PassedTestCases { get; set; }
    public int TotalTestCases { get; set; }
    public string? SubmittedCode { get; set; }
}
