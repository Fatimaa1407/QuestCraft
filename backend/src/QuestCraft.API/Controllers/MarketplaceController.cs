using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Marketplace;
using QuestCraft.Application.Features.Marketplace;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/marketplace")]
public class MarketplaceController : ControllerBase
{
    private readonly IMediator _mediator;

    public MarketplaceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("items")]
    public async Task<ActionResult<ApiResponse<List<MarketplaceItemDto>>>> GetItems([FromQuery] int? typeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMarketplaceItemsQuery(typeId), cancellationToken);
        return Ok(ApiResponse<List<MarketplaceItemDto>>.Ok(result));
    }

    [HttpGet("item-types")]
    public async Task<ActionResult<ApiResponse<List<ItemTypeDto>>>> GetItemTypes(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMarketplaceItemTypesQuery(), cancellationToken);
        return Ok(ApiResponse<List<ItemTypeDto>>.Ok(result));
    }

    [HttpPost("items")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<MarketplaceItemDto>>> CreateItem(CreateMarketplaceItemCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<MarketplaceItemDto>.Ok(result, "Məhsul yaradıldı."));
    }

    [HttpPut("items/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<MarketplaceItemDto>>> UpdateItem(int id, UpdateMarketplaceItemRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateMarketplaceItemCommand(
            id, request.Name, request.Description, request.ItemTypeId, request.Price, request.ImageUrl, request.IsActive,
            request.NameEn, request.DescriptionEn);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<MarketplaceItemDto>.Ok(result, "Məhsul yeniləndi."));
    }

    [HttpDelete("items/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteItem(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteMarketplaceItemCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Məhsul silindi."));
    }

    [HttpPost("items/{id:int}/purchase")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PurchaseResultDto>>> Purchase(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PurchaseItemCommand(id), cancellationToken);
        return Ok(ApiResponse<PurchaseResultDto>.Ok(result, "Alış uğurludur."));
    }

    [HttpPost("items/{id:int}/equip")]
    [Authorize]
    public async Task<IActionResult> Equip(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new EquipItemCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Taxıldı."));
    }

    [HttpPost("items/{id:int}/unequip")]
    [Authorize]
    public async Task<IActionResult> Unequip(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UnequipItemCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Çıxarıldı."));
    }

    [HttpGet("my-purchases")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<List<MyPurchaseDto>>>> GetMyPurchases(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyPurchasesQuery(), cancellationToken);
        return Ok(ApiResponse<List<MyPurchaseDto>>.Ok(result));
    }
}

public record UpdateMarketplaceItemRequest(
    string Name, string? Description, int ItemTypeId, int Price, string? ImageUrl, bool IsActive,
    string? NameEn = null, string? DescriptionEn = null);
