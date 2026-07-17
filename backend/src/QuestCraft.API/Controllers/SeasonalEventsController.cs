using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.SeasonalEvents;
using QuestCraft.Application.Features.Gamification;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/seasonal-events")]
public class SeasonalEventsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SeasonalEventsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<CurrentSeasonalEventDto?>>> GetCurrent(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCurrentSeasonalEventQuery(), cancellationToken);
        return Ok(ApiResponse<CurrentSeasonalEventDto?>.Ok(result));
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<SeasonalEventDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSeasonalEventsQuery(), cancellationToken);
        return Ok(ApiResponse<List<SeasonalEventDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SeasonalEventDto>>> Create(CreateSeasonalEventCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<SeasonalEventDto>.Ok(result, "Mövsümi hadisə yaradıldı."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<SeasonalEventDto>>> Update(int id, UpdateSeasonalEventRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new UpdateSeasonalEventCommand(id, request.Name, request.StartDate, request.EndDate, request.IsActive, request.NameEn, request.Description, request.DescriptionEn, request.Emoji),
            cancellationToken);
        return Ok(ApiResponse<SeasonalEventDto>.Ok(result, "Mövsümi hadisə yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteSeasonalEventCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Mövsümi hadisə silindi."));
    }
}

public record UpdateSeasonalEventRequest(
    string Name, DateOnly StartDate, DateOnly EndDate, bool IsActive,
    string? NameEn = null, string? Description = null, string? DescriptionEn = null, string? Emoji = null);
