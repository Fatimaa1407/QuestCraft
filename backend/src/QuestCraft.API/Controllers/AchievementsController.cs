using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Achievements;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/achievements")]
[Authorize(Roles = "Admin")]
public class AchievementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AchievementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<AchievementAdminDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAchievementsAdminQuery(), cancellationToken);
        return Ok(ApiResponse<List<AchievementAdminDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AchievementAdminDto>>> Create(CreateAchievementCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<AchievementAdminDto>.Ok(result, "Nailiyyət yaradıldı."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<AchievementAdminDto>>> Update(int id, UpdateAchievementRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateAchievementCommand(
            id, request.Name, request.NameEn, request.Description, request.DescriptionEn, request.IconUrl,
            request.ConditionType, request.ConditionValue, request.XpReward, request.CoinReward, request.IsActive);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<AchievementAdminDto>.Ok(result, "Nailiyyət yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteAchievementCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Nailiyyət silindi."));
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<ApiResponse<List<AchievementAdminDto>>>> GetDeleted(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletedAchievementsQuery(), cancellationToken);
        return Ok(ApiResponse<List<AchievementAdminDto>>.Ok(result));
    }

    [HttpPost("{id:int}/restore")]
    public async Task<ActionResult<ApiResponse<AchievementAdminDto>>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RestoreAchievementCommand(id), cancellationToken);
        return Ok(ApiResponse<AchievementAdminDto>.Ok(result, "Nailiyyət bərpa edildi."));
    }
}

public record UpdateAchievementRequest(
    string Name, string? NameEn, string Description, string? DescriptionEn, string? IconUrl,
    string ConditionType, int ConditionValue, int XpReward, int CoinReward, bool IsActive);
