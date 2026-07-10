using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExportUsersQuery : IQuery<byte[]>;

public class ExportUsersQueryHandler : IRequestHandler<ExportUsersQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelExportService;

    public ExportUsersQueryHandler(IApplicationDbContext context, IExcelExportService excelExportService)
    {
        _context = context;
        _excelExportService = excelExportService;
    }

    public async Task<byte[]> Handle(ExportUsersQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.Users
            .Include(u => u.Role)
            .Include(u => u.Profile)
            .OrderBy(u => u.Username)
            .Select(u => new
            {
                u.Username,
                u.Email,
                Role = u.Role.Name,
                Xp = u.Profile != null ? u.Profile.Xp : 0,
                Coins = u.Profile != null ? u.Profile.Coins : 0,
                Level = u.Profile != null ? u.Profile.Level : 1,
                u.IsActive,
                u.CreatedAt,
                u.LastLoginAt,
            })
            .ToListAsync(cancellationToken);

        return _excelExportService.Export(
            "Users",
            ["Username", "Email", "Role", "Xp", "Coins", "Level", "IsActive", "CreatedAt", "LastLoginAt"],
            rows,
            r => [r.Username, r.Email, r.Role, r.Xp, r.Coins, r.Level, r.IsActive, r.CreatedAt, r.LastLoginAt]);
    }
}
