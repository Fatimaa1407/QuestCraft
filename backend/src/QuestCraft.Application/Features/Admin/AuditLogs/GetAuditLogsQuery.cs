using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Common.Models;

namespace QuestCraft.Application.Features.Admin.AuditLogs;

public record AuditLogDto(int Id, int? UserId, string? Username, string Action, string EntityName, int? EntityId, string? NewValues, DateTime Timestamp, string? IpAddress);

public record GetAuditLogsQuery(int Page, int PageSize) : IQuery<PagedResult<AuditLogDto>>;

public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, PagedResult<AuditLogDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAuditLogsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public Task<PagedResult<AuditLogDto>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.AuditLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.Timestamp)
            .Select(a => new AuditLogDto(
                a.Id, a.UserId, a.User != null ? a.User.Username : null, a.Action, a.EntityName, a.EntityId, a.NewValues, a.Timestamp, a.IpAddress));

        return PagedResult<AuditLogDto>.CreateAsync(query, request.Page, request.PageSize, cancellationToken);
    }
}
