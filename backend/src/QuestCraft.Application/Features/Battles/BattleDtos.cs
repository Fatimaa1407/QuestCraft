namespace QuestCraft.Application.Features.Battles;

public record BattleParticipantDto(
    int UserId, string Username, string? AvatarUrl, bool HasFinished, DateTime? FinishedAt,
    int? Rank, int PassedTestCases, int TotalTestCases, string? FrameImageUrl);

public record BattleDto(
    int Id, string Mode, string Status, int ChallengeId, string ChallengeTitle, int TimeLimitMs,
    int HostUserId, int? InvitedUserId, string? JoinCode, int MaxPlayers,
    DateTime? StartedAt, DateTime? EndedAt, List<BattleParticipantDto> Participants);

public record BattleSummaryDto(int Id, string Mode, string Status, string ChallengeTitle, int PlayerCount, int MaxPlayers, string? JoinCode, DateTime CreatedAt);
