using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.Quizzes;
using QuestCraft.Application.Features.Quizzes;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizzesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizListItemDto>>>> GetAll(
        [FromQuery] int? categoryId,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var includeUnpublished = User.IsInRole("Admin");
        var result = await _mediator.Send(new GetQuizzesQuery(categoryId, search, page, pageSize, includeUnpublished), cancellationToken);
        return Ok(ApiResponse<PagedResult<QuizListItemDto>>.Ok(result));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<QuizAttemptViewDto>>> GetForAttempt(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuizForAttemptQuery(id), cancellationToken);
        return Ok(ApiResponse<QuizAttemptViewDto>.Ok(result));
    }

    [HttpGet("{id:int}/admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<QuizAdminDetailDto>>> GetForAdmin(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuizByIdAdminQuery(id), cancellationToken);
        return Ok(ApiResponse<QuizAdminDetailDto>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<QuizListItemDto>>> Create(CreateQuizCommand command, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<QuizListItemDto>.Ok(result, "Quiz yaradıldı."));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<QuizListItemDto>>> Update(int id, UpdateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateQuizCommand(id, request.Title, request.CategoryId, request.XpReward, request.IsPublished), cancellationToken);
        return Ok(ApiResponse<QuizListItemDto>.Ok(result, "Quiz yeniləndi."));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteQuizCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Quiz silindi."));
    }

    [HttpPost("{id:int}/questions")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<int>>> AddQuestion(int id, AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var command = new AddQuestionCommand(id, request.Text, request.Explanation, request.Options);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(ApiResponse<int>.Ok(result, "Sual əlavə edildi."));
    }

    [HttpPut("questions/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuestion(int id, UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new UpdateQuestionCommand(id, request.Text, request.Explanation, request.Options), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Sual yeniləndi."));
    }

    [HttpDelete("questions/{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuestion(int id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new DeleteQuestionCommand(id), cancellationToken);
        return Ok(ApiResponse<object?>.Ok(null, "Sual silindi."));
    }

    [HttpPost("{id:int}/attempt")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QuizAttemptResultDto>>> Attempt(int id, AttemptQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SubmitQuizAttemptCommand(id, request.Answers), cancellationToken);
        return Ok(ApiResponse<QuizAttemptResultDto>.Ok(result, "Quiz tamamlandı."));
    }

    [HttpGet("attempts/my")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PagedResult<QuizAttemptListItemDto>>>> GetMyAttempts(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetMyQuizAttemptsQuery(page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<QuizAttemptListItemDto>>.Ok(result));
    }

    [HttpGet("attempts/{id:int}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QuizAttemptResultDto>>> GetAttemptById(int id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuizAttemptByIdQuery(id), cancellationToken);
        return Ok(ApiResponse<QuizAttemptResultDto>.Ok(result));
    }
}

public record UpdateQuizRequest(string Title, int? CategoryId, int XpReward, bool IsPublished);

public record AddQuestionRequest(string Text, string? Explanation, List<QuestionOptionInput> Options);

public record UpdateQuestionRequest(string Text, string? Explanation, List<QuestionOptionInput> Options);

public record AttemptQuizRequest(List<QuizAnswerInput> Answers);
