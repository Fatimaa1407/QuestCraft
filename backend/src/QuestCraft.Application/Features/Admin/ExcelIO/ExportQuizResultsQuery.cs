using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExportQuizResultsQuery : IQuery<byte[]>;

public class ExportQuizResultsQueryHandler : IRequestHandler<ExportQuizResultsQuery, byte[]>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelExportService _excelExportService;

    public ExportQuizResultsQueryHandler(IApplicationDbContext context, IExcelExportService excelExportService)
    {
        _context = context;
        _excelExportService = excelExportService;
    }

    public async Task<byte[]> Handle(ExportQuizResultsQuery request, CancellationToken cancellationToken)
    {
        var rows = await _context.QuizAttempts
            .Include(a => a.User)
            .Include(a => a.Quiz)
            .OrderByDescending(a => a.CompletedAt)
            .Select(a => new { Username = a.User.Username, Quiz = a.Quiz.Title, a.Score, a.TotalQuestions, a.XpEarned, a.CompletedAt })
            .ToListAsync(cancellationToken);

        return _excelExportService.Export(
            "QuizResults",
            ["Username", "Quiz", "Score", "TotalQuestions", "XpEarned", "CompletedAt"],
            rows,
            r => [r.Username, r.Quiz, r.Score, r.TotalQuestions, r.XpEarned, r.CompletedAt]);
    }
}
