using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Challenges;
using QuestCraft.Application.Features.Challenges;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/challenges")]
public class ChallengesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ChallengesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ChallengeListItemDto>>>> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] int? difficultyId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var includeUnpublished = User.IsInRole("Admin");
        var result = await _mediator.Send(
            new GetChallengesQuery(categoryId, difficultyId, search, page, pageSize, includeUnpublished),
            cancellationToken);
        return Ok(ApiResponse<PagedResult<ChallengeListItemDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChallengeDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChallengeByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<ChallengeDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ChallengeDetailDto>>> Create(CreateChallengeCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ChallengeDetailDto>.Ok(result, "Challenge yaradıldı."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ChallengeDetailDto>>> Update(int id, UpdateChallengeRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateChallengeCommand(
            id, request.Title, request.Description, request.CategoryId, request.DifficultyId,
            request.TimeLimitMs, request.MemoryLimitMb, request.XpReward, request.CoinReward,
            request.StarterCode, request.Constraints, request.InputFormat, request.OutputFormat,
            request.SampleInput, request.SampleOutput, request.IsPublished, request.RequiredLevel,
            request.TitleEn, request.DescriptionEn, request.ConstraintsEn, request.InputFormatEn,
            request.OutputFormatEn, request.StarterCodeEn, request.Tags, request.IsBattleOnly);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<ChallengeDetailDto>.Ok(result, "Challenge yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteChallengeCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Challenge silindi."));
    }

    [HttpGet("battle-pool")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ChallengeListItemDto>>>> GetBattlePool(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetBattlePoolChallengesQuery(), cancellationToken);
        return Ok(ApiResponse<List<ChallengeListItemDto>>.Ok(result));
    }

    [HttpGet("deleted")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<List<ChallengeListItemDto>>>> GetDeleted(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDeletedChallengesQuery(), cancellationToken);
        return Ok(ApiResponse<List<ChallengeListItemDto>>.Ok(result));
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RestoreChallengeCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Challenge bərpa edildi."));
    }

    [HttpPost("{id:int}/test-cases")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> AddTestCase(int id, AddTestCaseRequest request, CancellationToken cancellationToken)
    {
        var command = new AddTestCaseCommand(id, request.Input, request.ExpectedOutput, request.OrderIndex, request.IsHidden, request.Weight);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<int>.Ok(result, "Test case əlavə edildi."));
    }

    [HttpPut("test-cases/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTestCase(int id, UpdateTestCaseRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateTestCaseCommand(id, request.IsHidden, request.Input, request.ExpectedOutput, request.OrderIndex, request.Weight);
        await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Test case yeniləndi."));
    }

    [HttpDelete("test-cases/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTestCase(int id, [FromQuery] bool isHidden, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteTestCaseCommand(id, isHidden), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Test case silindi."));
    }

    [HttpGet("{id:int}/stats")]
    public async Task<ActionResult<ApiResponse<ChallengeStatsDto>>> GetStats(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChallengeStatsQuery(id), cancellationToken);
        return Ok(ApiResponse<ChallengeStatsDto>.Ok(result));
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<ApiResponse<List<ChallengeCommentThreadDto>>>> GetComments(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetChallengeCommentsQuery(id), cancellationToken);
        return Ok(ApiResponse<List<ChallengeCommentThreadDto>>.Ok(result));
    }

    [HttpPost("{id:int}/comments")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<ChallengeCommentDto>>> PostComment(int id, PostCommentRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new PostChallengeCommentCommand(id, request.Content, request.IsSpoiler, request.ParentCommentId), cancellationToken);
        return Ok(ApiResponse<ChallengeCommentDto>.Ok(result, "Şərh əlavə edildi."));
    }

    [HttpGet("daily-puzzle")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DailyPuzzleDto>>> GetDailyPuzzle(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDailyPuzzleQuery(), cancellationToken);
        return Ok(ApiResponse<DailyPuzzleDto>.Ok(result));
    }
}

public record PostCommentRequest(string Content, bool IsSpoiler, int? ParentCommentId = null);

public record UpdateChallengeRequest(
    string Title,
    string Description,
    int CategoryId,
    int DifficultyId,
    int TimeLimitMs,
    int MemoryLimitMb,
    int XpReward,
    int CoinReward,
    string StarterCode,
    string? Constraints,
    string? InputFormat,
    string? OutputFormat,
    string? SampleInput,
    string? SampleOutput,
    bool IsPublished,
    int RequiredLevel = 1,
    string? TitleEn = null,
    string? DescriptionEn = null,
    string? ConstraintsEn = null,
    string? InputFormatEn = null,
    string? OutputFormatEn = null,
    string? StarterCodeEn = null,
    string? Tags = null,
    bool IsBattleOnly = false);

public record AddTestCaseRequest(string Input, string ExpectedOutput, int OrderIndex, bool IsHidden, int Weight = 1);

public record UpdateTestCaseRequest(bool IsHidden, string Input, string ExpectedOutput, int OrderIndex, int Weight = 1);
