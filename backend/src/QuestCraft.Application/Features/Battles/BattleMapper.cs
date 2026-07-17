using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Battles;

public static class BattleMapper
{
    // Expects Challenge and Participants (with each participant's User + User.Profile) already loaded.
    public static BattleDto ToDto(Battle battle) => new(
        battle.Id,
        battle.Mode.ToString(),
        battle.Status.ToString(),
        battle.ChallengeId,
        battle.Challenge.Title,
        battle.Challenge.TimeLimitMs,
        battle.HostUserId,
        battle.InvitedUserId,
        battle.JoinCode,
        battle.MaxPlayers,
        battle.StartedAt,
        battle.EndedAt,
        battle.Participants
            .OrderBy(p => p.Rank ?? int.MaxValue)
            .ThenBy(p => p.CreatedAt)
            .Select(p => new BattleParticipantDto(
                p.UserId,
                p.User.Username,
                p.User.Profile != null ? p.User.Profile.AvatarUrl : null,
                p.HasFinished,
                p.FinishedAt,
                p.Rank,
                p.PassedTestCases,
                p.TotalTestCases))
            .ToList());
}
