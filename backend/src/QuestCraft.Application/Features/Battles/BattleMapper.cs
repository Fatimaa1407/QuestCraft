using QuestCraft.Application.Common;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Battles;

public static class BattleMapper
{
    // Expects Challenge and Participants (with each participant's User + User.Profile, including
    // EquippedAvatar/EquippedFrame/EquippedTitle/EquippedBadge navigations) already loaded.
    public static BattleDto ToDto(Battle battle, bool isEnglish = false) => new(
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
                p.User.Profile != null ? (p.User.Profile.EquippedAvatar != null ? p.User.Profile.EquippedAvatar.ImageUrl : p.User.Profile.AvatarUrl) : null,
                p.HasFinished,
                p.FinishedAt,
                p.Rank,
                p.PassedTestCases,
                p.TotalTestCases,
                p.User.Profile != null && p.User.Profile.EquippedFrame != null ? p.User.Profile.EquippedFrame.ImageUrl : null,
                p.User.Profile != null && p.User.Profile.EquippedTitle != null
                    ? LocalizationHelper.Pick(p.User.Profile.EquippedTitle.Name, p.User.Profile.EquippedTitle.NameEn, isEnglish)
                    : null,
                p.User.Profile != null && p.User.Profile.EquippedBadge != null ? p.User.Profile.EquippedBadge.ImageUrl : null,
                p.User.Profile != null && p.User.Profile.EquippedBadge != null
                    ? LocalizationHelper.Pick(p.User.Profile.EquippedBadge.Name, p.User.Profile.EquippedBadge.NameEn, isEnglish)
                    : null))
            .ToList());
}
