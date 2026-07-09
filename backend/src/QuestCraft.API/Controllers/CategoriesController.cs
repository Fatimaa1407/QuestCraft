using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Categories;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoriesQuery(), cancellationToken);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yaradıldı."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Update(int id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateCategoryCommand(id, request.Name, request.Description, request.IconUrl), cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Kateqoriya silindi."));
    }

    [HttpGet("deleted")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetDeleted(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletedCategoriesQuery(), cancellationToken);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(result));
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RestoreCategoryCommand(id), cancellationToken);
        return Ok(ApiResponse<CategoryDto>.Ok(result, "Kateqoriya bərpa edildi."));
    }
}

public record UpdateCategoryRequest(string Name, string? Description, string? IconUrl);
