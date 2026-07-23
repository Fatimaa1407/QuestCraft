using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Submissions;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubmissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("run")]
    public async Task<ActionResult<ApiResponse<RunResultDto>>> Run(RunRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RunChallengeQuery(request.ChallengeId, request.SourceCode), cancellationToken);
        return Ok(ApiResponse<RunResultDto>.Ok(result));
    }

    [HttpPost("submit")]
    public async Task<ActionResult<ApiResponse<SubmissionResultDto>>> Submit(SubmitRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitChallengeCommand(request.ChallengeId, request.SourceCode, request.SolveTimeMs), cancellationToken);
        var message = result.Verdict == "Accepted" ? "Təbriklər, qəbul edildi!" : "Göndərildi.";
        return Ok(ApiResponse<SubmissionResultDto>.Ok(result, message));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<SubmissionResultDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetSubmissionByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<SubmissionResultDto>.Ok(result));
    }

    [HttpGet("my")]
    public async Task<ActionResult<ApiResponse<PagedResult<SubmissionListItemDto>>>> GetMy(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMySubmissionsQuery(page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<SubmissionListItemDto>>.Ok(result));
    }

    [HttpGet("challenge/{challengeId:int}")]
    public async Task<ActionResult<ApiResponse<ChallengeReplayDto>>> GetChallengeReplay(int challengeId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChallengeReplayQuery(challengeId), cancellationToken);
        return Ok(ApiResponse<ChallengeReplayDto>.Ok(result));
    }
}

public record RunRequest(int ChallengeId, string SourceCode);

public record SubmitRequest(int ChallengeId, string SourceCode, int? SolveTimeMs = null);
