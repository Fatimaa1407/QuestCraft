using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Difficulties;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/difficulties")]
public class DifficultiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DifficultiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DifficultyDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDifficultiesQuery(), cancellationToken);
        return Ok(ApiResponse<List<DifficultyDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DifficultyDto>>> Create(CreateDifficultyCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<DifficultyDto>.Ok(result, "Çətinlik dərəcəsi yaradıldı."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DifficultyDto>>> Update(int id, UpdateDifficultyRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateDifficultyCommand(id, request.Name, request.Color, request.XpMultiplier), cancellationToken);
        return Ok(ApiResponse<DifficultyDto>.Ok(result, "Çətinlik dərəcəsi yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDifficultyCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Çətinlik dərəcəsi silindi."));
    }

    [HttpGet("deleted")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<DifficultyDto>>>> GetDeleted(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletedDifficultiesQuery(), cancellationToken);
        return Ok(ApiResponse<List<DifficultyDto>>.Ok(result));
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DifficultyDto>>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RestoreDifficultyCommand(id), cancellationToken);
        return Ok(ApiResponse<DifficultyDto>.Ok(result, "Çətinlik dərəcəsi bərpa edildi."));
    }
}

public record UpdateDifficultyRequest(string Name, string? Color, double XpMultiplier);
