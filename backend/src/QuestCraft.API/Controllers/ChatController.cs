using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Chat;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<ConversationDto>>>> GetConversations(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetConversationsQuery(), cancellationToken);
        return Ok(ApiResponse<List<ConversationDto>>.Ok(result));
    }

    [HttpGet("{userId:int}")]
    public async Task<ActionResult<ApiResponse<PagedResult<ChatMessageDto>>>> GetConversation(
        int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 30, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetConversationQuery(userId, page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<ChatMessageDto>>.Ok(result));
    }

    [HttpPost("{userId:int}")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage(int userId, SendChatMessageRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendChatMessageCommand(userId, request.Content), cancellationToken);
        return Ok(ApiResponse<ChatMessageDto>.Ok(result));
    }

    [HttpPost("{userId:int}/read")]
    public async Task<IActionResult> MarkRead(int userId, CancellationToken cancellationToken)
    {
        await _mediator.Send(new MarkConversationReadCommand(userId), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null));
    }
}

public record SendChatMessageRequest(string Content);
