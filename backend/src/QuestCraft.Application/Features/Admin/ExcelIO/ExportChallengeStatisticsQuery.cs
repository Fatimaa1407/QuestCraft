using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Enums;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExportChallengeStatisticsQuery : IQuery<byte[]>;

public class ExportChallengeStatisticsQueryHandler : IRequestHandler<ExportChallengeStatisticsQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelExportService;

    public ExportChallengeStatisticsQueryHandler(IApplicationDbContext context, IExcelExportService excelExportService)
    {
        _context = context;
        _excelExportService = excelExportService;
    }

    public async Task<byte[]> Handle(ExportChallengeStatisticsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.Challenges
            .Include(c => c.Category)
            .Include(c => c.Difficulty)
            .OrderByDescending(c => c.Submissions.Count)
            .Select(c => new
            {
                c.Title,
                Category = c.Category.Name,
                Difficulty = c.Difficulty.Name,
                TotalSubmissions = c.Submissions.Count,
                AcceptedSubmissions = c.Submissions.Count(s => s.Verdict == SubmissionVerdict.Accepted),
                c.IsPublished,
            })
            .ToListAsync(cancellationToken);

        return _excelExportService.Export(
            "ChallengeStatistics",
            ["Title", "Category", "Difficulty", "TotalSubmissions", "AcceptedSubmissions", "AcceptRate%", "IsPublished"],
            rows,
            r => [r.Title, r.Category, r.Difficulty, r.TotalSubmissions, r.AcceptedSubmissions,
                r.TotalSubmissions == 0 ? 0 : Math.Round(100.0 * r.AcceptedSubmissions / r.TotalSubmissions, 1), r.IsPublished]);
    }
}
