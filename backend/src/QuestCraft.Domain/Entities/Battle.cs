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

    // Denormalized copy of Participants.Count, written on every join. Joining a Room battle only
    // inserts a BattleParticipant row and never otherwise touches the Battle row itself, so without
    // this the Version concurrency token below would never engage for a same-slot join race.
    public int ParticipantCount { get; set; } = 1;

    // Optimistic-concurrency token: guards against two participants simultaneously joining the last
    // open slot, and against two simultaneous full solves both computing themselves as Rank 1. A
    // plain, manually-incremented counter rather than a DB-generated rowversion column, so the same
    // check behaves identically on SQL Server, SQLite (integration tests) and the InMemory provider
    // (unit tests) — none of which need to auto-generate it, callers just bump it whenever they touch
    // the row in a way another concurrent request must not silently clobber.
    public int Version { get; set; }

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
