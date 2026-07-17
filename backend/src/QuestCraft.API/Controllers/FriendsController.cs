using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Friends;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/friends")]
[Authorize]
public class FriendsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FriendsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<FriendDto>>>> GetFriends(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetFriendsQuery(), cancellationToken);
        return Ok(ApiResponse<List<FriendDto>>.Ok(result));
    }

    [HttpGet("requests")]
    public async Task<ActionResult<ApiResponse<List<FriendRequestDto>>>> GetIncomingRequests(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetIncomingFriendRequestsQuery(), cancellationToken);
        return Ok(ApiResponse<List<FriendRequestDto>>.Ok(result));
    }

    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<List<UserSearchResultDto>>>> Search([FromQuery] string query, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SearchUsersQuery(query), cancellationToken);
        return Ok(ApiResponse<List<UserSearchResultDto>>.Ok(result));
    }

    [HttpPost("requests")]
    public async Task<IActionResult> SendRequest(SendFriendRequestRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SendFriendRequestCommand(request.AddresseeUserId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Dostluq istəyi göndərildi."));
    }

    [HttpPost("requests/{id:int}/respond")]
    public async Task<IActionResult> RespondRequest(int id, RespondFriendRequestRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RespondFriendRequestCommand(id, request.Accept), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, request.Accept ? "Dostluq istəyi qəbul edildi." : "Dostluq istəyi rədd edildi."));
    }

    [HttpDelete("{userId:int}")]
    public async Task<IActionResult> RemoveFriend(int userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoveFriendCommand(userId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Dost siyahıdan silindi."));
    }
}

public record SendFriendRequestRequest(int AddresseeUserId);
public record RespondFriendRequestRequest(bool Accept);
