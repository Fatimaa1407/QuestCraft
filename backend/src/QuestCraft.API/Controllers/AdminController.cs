using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.AuditLogs;
using QuestCraft.Application.Features.Admin.ExcelIO;
using QuestCraft.Domain.Enums;

namespace QuestCraft.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }

    [HttpPost("excel/import/challenges")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ExcelImportResultDto>>> ImportChallenges(IFormFile file, CancellationToken cancellationToken)
    {
        var content = await ReadFileAsync(file);
        var result = await _mediator.Send(new ImportChallengesCommand(file.FileName, content), cancellationToken);
        return Ok(ApiResponse<ExcelImportResultDto>.Ok(result, $"{result.SuccessRows}/{result.TotalRows} sətir import edildi."));
    }

    [HttpPost("excel/import/challenges/{challengeId:int}/test-cases")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ExcelImportResultDto>>> ImportTestCases(int challengeId, IFormFile file, CancellationToken cancellationToken)
    {
        var content = await ReadFileAsync(file);
        var result = await _mediator.Send(new ImportTestCasesCommand(challengeId, file.FileName, content), cancellationToken);
        return Ok(ApiResponse<ExcelImportResultDto>.Ok(result, $"{result.SuccessRows}/{result.TotalRows} sətir import edildi."));
    }

    [HttpPost("excel/import/quizzes/{quizId:int}/questions")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ApiResponse<ExcelImportResultDto>>> ImportQuizQuestions(int quizId, IFormFile file, CancellationToken cancellationToken)
    {
        var content = await ReadFileAsync(file);
        var result = await _mediator.Send(new ImportQuizQuestionsCommand(quizId, file.FileName, content), cancellationToken);
        return Ok(ApiResponse<ExcelImportResultDto>.Ok(result, $"{result.SuccessRows}/{result.TotalRows} sətir import edildi."));
    }

    [HttpGet("excel/export/users")]
    public async Task<IActionResult> ExportUsers(CancellationToken cancellationToken)
    {
        var bytes = await _mediator.Send(new ExportUsersQuery(), cancellationToken);
        return ExcelFile(bytes, "users.xlsx");
    }

    [HttpGet("excel/export/challenges")]
    public async Task<IActionResult> ExportChallengeStatistics(CancellationToken cancellationToken)
    {
        var bytes = await _mediator.Send(new ExportChallengeStatisticsQuery(), cancellationToken);
        return ExcelFile(bytes, "challenge-statistics.xlsx");
    }

    [HttpGet("excel/export/quiz-results")]
    public async Task<IActionResult> ExportQuizResults(CancellationToken cancellationToken)
    {
        var bytes = await _mediator.Send(new ExportQuizResultsQuery(), cancellationToken);
        return ExcelFile(bytes, "quiz-results.xlsx");
    }

    [HttpGet("excel/export/leaderboard")]
    public async Task<IActionResult> ExportLeaderboard([FromQuery] LeaderboardPeriod period = LeaderboardPeriod.AllTime, CancellationToken cancellationToken = default)
    {
        var bytes = await _mediator.Send(new ExportLeaderboardQuery(period), cancellationToken);
        return ExcelFile(bytes, $"leaderboard-{period}.xlsx");
    }

    [HttpGet("excel/export/marketplace")]
    public async Task<IActionResult> ExportMarketplaceData(CancellationToken cancellationToken)
    {
        var bytes = await _mediator.Send(new ExportMarketplaceDataQuery(), cancellationToken);
        return ExcelFile(bytes, "marketplace-data.xlsx");
    }

    private static async Task<byte[]> ReadFileAsync(IFormFile file)
    {
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return stream.ToArray();
    }

    private FileContentResult ExcelFile(byte[] bytes, string fileName) =>
        File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
}
