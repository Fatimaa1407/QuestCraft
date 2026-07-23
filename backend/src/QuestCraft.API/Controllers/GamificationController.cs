using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Enums;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/gamification")]
public class GamificationController : ControllerBase
{
    private readonly IMediator _mediator;

    public GamificationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("daily-quests")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<DailyQuestDto>>>> GetDailyQuests(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDailyQuestsQuery(), cancellationToken);
        return Ok(ApiResponse<List<DailyQuestDto>>.Ok(result));
    }

    [HttpPost("daily-quests/{id:int}/claim")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ClaimDailyQuestResultDto>>> ClaimDailyQuest(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ClaimDailyQuestRewardCommand(id), cancellationToken);
        return Ok(ApiResponse<ClaimDailyQuestResultDto>.Ok(result, "Mükafat alındı."));
    }

    [HttpGet("level-progress")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<LevelProgressDto>>> GetLevelProgress(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLevelProgressQuery(), cancellationToken);
        return Ok(ApiResponse<LevelProgressDto>.Ok(result));
    }

    [HttpGet("achievements")]
    public async Task<ActionResult<ApiResponse<List<AchievementDto>>>> GetAchievements(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAchievementsQuery(), cancellationToken);
        return Ok(ApiResponse<List<AchievementDto>>.Ok(result));
    }

    [HttpGet("leaderboard")]
    public async Task<ActionResult<ApiResponse<List<LeaderboardEntryDto>>>> GetLeaderboard(
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime,
        [FromQuery] int top = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetLeaderboardQuery(period, top), cancellationToken);
        return Ok(ApiResponse<List<LeaderboardEntryDto>>.Ok(result));
    }

    [HttpGet("analytics")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DashboardAnalyticsDto>>> GetDashboardAnalytics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardAnalyticsQuery(), cancellationToken);
        return Ok(ApiResponse<DashboardAnalyticsDto>.Ok(result));
    }

    [HttpGet("streak")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<StreakDto>>> GetMyStreak(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyStreakQuery(), cancellationToken);
        return Ok(ApiResponse<StreakDto>.Ok(result));
    }

    [HttpGet("my-rank")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MyRankDto>>> GetMyRank(
        [FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyRankQuery(period), cancellationToken);
        return Ok(ApiResponse<MyRankDto>.Ok(result));
    }

    [HttpPost("daily-login-reward/claim")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DailyLoginRewardDto>>> ClaimDailyLoginReward(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new ClaimDailyLoginRewardCommand(), cancellationToken);
        return Ok(ApiResponse<DailyLoginRewardDto>.Ok(result));
    }

    [HttpGet("statistics")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MyStatisticsDto>>> GetMyStatistics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyStatisticsQuery(), cancellationToken);
        return Ok(ApiResponse<MyStatisticsDto>.Ok(result));
    }

    [HttpPost("achievements/{id:int}/pin")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object?>>> PinAchievement(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new PinAchievementCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Sancıldı."));
    }

    [HttpPost("achievements/{id:int}/unpin")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<object?>>> UnpinAchievement(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnpinAchievementCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Çıxarıldı."));
    }

    [HttpGet("recommendations")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<RecommendationDto>>> GetRecommendations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetRecommendationsQuery(), cancellationToken);
        return Ok(ApiResponse<RecommendationDto>.Ok(result));
    }

    [HttpGet("heatmap")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<HeatmapDayDetailDto>>>> GetActivityHeatmap([FromQuery] int days = 180, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetActivityHeatmapQuery(days), cancellationToken);
        return Ok(ApiResponse<List<HeatmapDayDetailDto>>.Ok(result));
    }

    [HttpGet("certificate")]
    [Authorize]
    public async Task<IActionResult> GetCertificate(CancellationToken cancellationToken)
    {
        var bytes = await _mediator.Send(new GenerateCertificateQuery(), cancellationToken);
        return File(bytes, "application/pdf", "questcraft-certificate.pdf");
    }
}
