using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Profile;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<MyProfileDto>>> GetMy(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(ApiResponse<MyProfileDto>.Ok(result));
    }

    [HttpPut]
    public async Task<ActionResult<ApiResponse<MyProfileDto>>> UpdateMy(UpdateOwnProfileCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<MyProfileDto>.Ok(result, "Profil yeniləndi."));
    }

    [HttpGet("me/equipped")]
    public async Task<ActionResult<ApiResponse<EquippedCosmeticsDto>>> GetMyEquipped(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMyEquippedCosmeticsQuery(), cancellationToken);
        return Ok(ApiResponse<EquippedCosmeticsDto>.Ok(result));
    }
}
