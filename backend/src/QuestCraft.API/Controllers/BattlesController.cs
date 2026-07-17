using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Battles;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/battles")]
[Authorize]
public class BattlesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BattlesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("mine")]
    public async Task<ActionResult<ApiResponse<List<BattleSummaryDto>>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyActiveBattlesQuery(), cancellationToken);
        return Ok(ApiResponse<List<BattleSummaryDto>>.Ok(result));
    }

    [HttpGet("open-rooms")]
    public async Task<ActionResult<ApiResponse<List<BattleSummaryDto>>>> GetOpenRooms(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetOpenRoomsQuery(), cancellationToken);
        return Ok(ApiResponse<List<BattleSummaryDto>>.Ok(result));
    }

    [HttpGet("by-code/{code}")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBattleByCodeQuery(code), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBattleQuery(id), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result));
    }

    [HttpPost("duel")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> CreateDuel(CreateDuelRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateDuelBattleCommand(request.OpponentUserId), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result, "Duel dəvəti göndərildi."));
    }

    [HttpPost("room")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> CreateRoom(CreateRoomRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new CreateRoomBattleCommand(request.MaxPlayers), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result, "Otaq yaradıldı."));
    }

    [HttpPost("{id:int}/join")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> Join(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new JoinBattleCommand(id), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result, "Döyüşə qoşuldunuz."));
    }

    [HttpPost("{id:int}/start")]
    public async Task<ActionResult<ApiResponse<BattleDto>>> Start(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new StartRoomBattleCommand(id), cancellationToken);
        return Ok(ApiResponse<BattleDto>.Ok(result, "Döyüş başladı."));
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new CancelBattleCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Döyüş ləğv edildi."));
    }

    [HttpPost("{id:int}/submit")]
    public async Task<ActionResult<ApiResponse<BattleSubmissionResultDto>>> Submit(int id, SubmitBattleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitBattleSolutionCommand(id, request.SourceCode), cancellationToken);
        return Ok(ApiResponse<BattleSubmissionResultDto>.Ok(result));
    }
}

public record CreateDuelRequest(int OpponentUserId);
public record CreateRoomRequest(int MaxPlayers);
public record SubmitBattleRequest(string SourceCode);
