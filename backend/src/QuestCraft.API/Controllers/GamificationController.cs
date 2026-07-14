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
}
