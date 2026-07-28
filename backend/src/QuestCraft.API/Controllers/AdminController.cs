using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestCraft.Application.Common.Models;
using QuestCraft.Application.Features.Admin.AuditLogs;
using QuestCraft.Application.Features.Admin.Dashboard;
using QuestCraft.Application.Features.Admin.Users;
using QuestCraft.Application.Features.Gamification;

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

    [HttpGet("dashboard-summary")]
    public async Task<ActionResult<ApiResponse<AdminDashboardSummaryDto>>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminDashboardSummaryQuery(), cancellationToken);
        return Ok(ApiResponse<AdminDashboardSummaryDto>.Ok(result));
    }

    [HttpPost("weekly-recap/run")]
    public async Task<ActionResult<ApiResponse<int>>> RunWeeklyRecap([FromQuery] int? userId, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new SendWeeklyRecapCommand(userId, Force: true), cancellationToken);
        return Ok(ApiResponse<int>.Ok(result, $"{result} istifadəçiyə icmal göndərildi."));
    }

    [HttpGet("activity-today")]
    public async Task<ActionResult<ApiResponse<List<AdminActivityItemDto>>>> GetActivityToday(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAdminActivityTodayQuery(), cancellationToken);
        return Ok(ApiResponse<List<AdminActivityItemDto>>.Ok(result));
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<PagedResult<AdminUserListItemDto>>>> GetUsers(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAdminUsersQuery(page, pageSize, search), cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminUserListItemDto>>.Ok(result));
    }

    [HttpPatch("users/{id:int}/role")]
    public async Task<ActionResult<ApiResponse<AdminUserListItemDto>>> UpdateUserRole(int id, [FromBody] UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateUserRoleCommand(id, request.Role), cancellationToken);
        return Ok(ApiResponse<AdminUserListItemDto>.Ok(result, "İstifadəçinin rolu yeniləndi."));
    }

    [HttpPatch("users/{id:int}/active")]
    public async Task<ActionResult<ApiResponse<AdminUserListItemDto>>> UpdateUserActive(int id, [FromBody] UpdateUserActiveRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new UpdateUserActiveCommand(id, request.IsActive), cancellationToken);
        return Ok(ApiResponse<AdminUserListItemDto>.Ok(result, request.IsActive ? "İstifadəçi aktivləşdirildi." : "İstifadəçi deaktiv edildi."));
    }

    [HttpGet("audit-logs")]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogDto>>>> GetAuditLogs(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery(page, pageSize), cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result));
    }
}

public record UpdateUserRoleRequest(string Role);
public record UpdateUserActiveRequest(bool IsActive);
