using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

// Column order: Input, ExpectedOutput, OrderIndex, IsHidden, Weight
public record ImportTestCasesCommand(int ChallengeId, string FileName, byte[] FileContent) : ICommand<ExcelImportResultDto>;

public class ImportTestCasesCommandHandler : IRequestHandler<ImportTestCasesCommand, ExcelImportResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelReader _excelReader;
    private readonly ICurrentUserService _currentUser;

    public ImportTestCasesCommandHandler(IApplicationDbContext context, IExcelReader excelReader, ICurrentUserService currentUser)
    {
        _context = context;
        _excelReader = excelReader;
        _currentUser = currentUser;
    }

    public async Task<ExcelImportResultDto> Handle(ImportTestCasesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var challengeExists = await _context.Challenges.AnyAsync(c => c.Id == request.ChallengeId, cancellationToken);
        if (!challengeExists)
        {
            throw new NotFoundException(nameof(Challenge), request.ChallengeId);
        }

        var rows = _excelReader.ReadRows(request.FileContent);
        var errors = new List<string>();
        var successCount = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var input = ExcelRowReader.RequiredCell(row, 0, "Input");
                var expectedOutput = ExcelRowReader.RequiredCell(row, 1, "ExpectedOutput");
                var orderIndex = ExcelRowReader.ParseInt(row, 2, i);
                var isHidden = ExcelRowReader.ParseBool(row, 3);
                var weight = ExcelRowReader.ParseInt(row, 4, 1);

                if (isHidden)
                {
                    _context.HiddenTestCases.Add(new HiddenTestCase
                    {
                        ChallengeId = request.ChallengeId,
                        Input = input,
                        ExpectedOutput = expectedOutput,
                        OrderIndex = orderIndex,
                        Weight = weight,
                    });
                }
                else
                {
                    _context.TestCases.Add(new TestCase
                    {
                        ChallengeId = request.ChallengeId,
                        Input = input,
                        ExpectedOutput = expectedOutput,
                        OrderIndex = orderIndex,
                    });
                }

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
            EntityType = "TestCases",
            TotalRows = rows.Count,
            SuccessRows = successCount,
            FailedRows = errors.Count,
            ErrorDetails = errors.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(errors) : null,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return new ExcelImportResultDto(rows.Count, successCount, errors.Count, errors);
    }
}
