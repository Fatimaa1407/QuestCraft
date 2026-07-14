using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.DailyQuestTemplates;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/daily-quest-templates")]
[Authorize(Roles = "Admin")]
public class DailyQuestTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public DailyQuestTemplatesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DailyQuestTemplateAdminDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDailyQuestTemplatesAdminQuery(), cancellationToken);
        return Ok(ApiResponse<List<DailyQuestTemplateAdminDto>>.Ok(result));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DailyQuestTemplateAdminDto>>> Create(CreateDailyQuestTemplateCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<DailyQuestTemplateAdminDto>.Ok(result, "Gündəlik tapşırıq yaradıldı."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<DailyQuestTemplateAdminDto>>> Update(int id, UpdateDailyQuestTemplateRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateDailyQuestTemplateCommand(
            id, request.Title, request.TitleEn, request.Description, request.DescriptionEn,
            request.TargetType, request.TargetValue, request.XpReward, request.CoinReward, request.IsActive);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<DailyQuestTemplateAdminDto>.Ok(result, "Gündəlik tapşırıq yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteDailyQuestTemplateCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Gündəlik tapşırıq silindi."));
    }

    [HttpGet("deleted")]
    public async Task<ActionResult<ApiResponse<List<DailyQuestTemplateAdminDto>>>> GetDeleted(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletedDailyQuestTemplatesQuery(), cancellationToken);
        return Ok(ApiResponse<List<DailyQuestTemplateAdminDto>>.Ok(result));
    }

    [HttpPost("{id:int}/restore")]
    public async Task<ActionResult<ApiResponse<DailyQuestTemplateAdminDto>>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RestoreDailyQuestTemplateCommand(id), cancellationToken);
        return Ok(ApiResponse<DailyQuestTemplateAdminDto>.Ok(result, "Gündəlik tapşırıq bərpa edildi."));
    }
}

public record UpdateDailyQuestTemplateRequest(
    string Title, string? TitleEn, string? Description, string? DescriptionEn,
    string TargetType, int TargetValue, int XpReward, int CoinReward, bool IsActive);
