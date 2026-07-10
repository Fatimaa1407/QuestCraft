using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

// Column order: Title, Description, CategoryName, DifficultyName, TimeLimitMs, MemoryLimitMb,
// XpReward, CoinReward, StarterCode, Constraints, InputFormat, OutputFormat, SampleInput, SampleOutput, Hint, IsPublished
public record ImportChallengesCommand(string FileName, byte[] FileContent) : ICommand<ExcelImportResultDto>;

public class ImportChallengesCommandHandler : IRequestHandler<ImportChallengesCommand, ExcelImportResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelReader _excelReader;
    private readonly ICurrentUserService _currentUser;

    public ImportChallengesCommandHandler(IApplicationDbContext context, IExcelReader excelReader, ICurrentUserService currentUser)
    {
        _context = context;
        _excelReader = excelReader;
        _currentUser = currentUser;
    }

    public async Task<ExcelImportResultDto> Handle(ImportChallengesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");
        var rows = _excelReader.ReadRows(request.FileContent);

        var categories = await _context.ChallengeCategories.ToDictionaryAsync(c => c.Name, c => c.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);
        var difficulties = await _context.ChallengeDifficulties.ToDictionaryAsync(d => d.Name, d => d.Id, StringComparer.OrdinalIgnoreCase, cancellationToken);

        var errors = new List<string>();
        var successCount = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var title = ExcelRowReader.RequiredCell(row, 0, "Title");
                var categoryName = ExcelRowReader.RequiredCell(row, 2, "CategoryName");
                if (!categories.TryGetValue(categoryName, out var categoryId))
                {
                    throw new InvalidOperationException($"Kateqoriya tapılmadı: {categoryName}");
                }

                var difficultyName = ExcelRowReader.RequiredCell(row, 3, "DifficultyName");
                if (!difficulties.TryGetValue(difficultyName, out var difficultyId))
                {
                    throw new InvalidOperationException($"Çətinlik dərəcəsi tapılmadı: {difficultyName}");
                }

                _context.Challenges.Add(new Challenge
                {
                    Title = title,
                    Description = ExcelRowReader.Cell(row, 1) ?? string.Empty,
                    CategoryId = categoryId,
                    DifficultyId = difficultyId,
                    TimeLimitMs = ExcelRowReader.ParseInt(row, 4, 2000),
                    MemoryLimitMb = ExcelRowReader.ParseInt(row, 5, 256),
                    XpReward = ExcelRowReader.ParseInt(row, 6, 0),
                    CoinReward = ExcelRowReader.ParseInt(row, 7, 0),
                    StarterCode = ExcelRowReader.Cell(row, 8) ?? string.Empty,
                    Constraints = ExcelRowReader.Cell(row, 9),
                    InputFormat = ExcelRowReader.Cell(row, 10),
                    OutputFormat = ExcelRowReader.Cell(row, 11),
                    SampleInput = ExcelRowReader.Cell(row, 12),
                    SampleOutput = ExcelRowReader.Cell(row, 13),
                    Hint = ExcelRowReader.Cell(row, 14),
                    IsPublished = ExcelRowReader.ParseBool(row, 15),
                });

                successCount++;
            }
            catch (Exception ex)
            {
                errors.Add($"Sətir {i + 2}: {ex.Message}");
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _context.ExcelImportLogs.Add(new ExcelImportLog
        {
            UserId = userId,
            FileName = request.FileName,
            EntityType = "Challenges",
            TotalRows = rows.Count,
            SuccessRows = successCount,
            FailedRows = errors.Count,
            ErrorDetails = errors.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(errors) : null,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return new ExcelImportResultDto(rows.Count, successCount, errors.Count, errors);
    }
}
