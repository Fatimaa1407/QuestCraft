using MediatR;
using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Domain.Entities;

namespace QuestCraft.Application.Features.Admin.ExcelIO;

// Column order: QuestionText, Explanation, Option1, IsCorrect1, Option2, IsCorrect2, Option3, IsCorrect3, Option4, IsCorrect4
// (Option3/4 columns may be left empty for 2-option questions.)
public record ImportQuizQuestionsCommand(int QuizId, string FileName, byte[] FileContent) : ICommand<ExcelImportResultDto>;

public class ImportQuizQuestionsCommandHandler : IRequestHandler<ImportQuizQuestionsCommand, ExcelImportResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IExcelReader _excelReader;
    private readonly ICurrentUserService _currentUser;

    public ImportQuizQuestionsCommandHandler(IApplicationDbContext context, IExcelReader excelReader, ICurrentUserService currentUser)
    {
        _context = context;
        _excelReader = excelReader;
        _currentUser = currentUser;
    }

    public async Task<ExcelImportResultDto> Handle(ImportQuizQuestionsCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("İstifadəçi tanınmadı.");

        var quizExists = await _context.Quizzes.AnyAsync(q => q.Id == request.QuizId, cancellationToken);
        if (!quizExists)
        {
            throw new NotFoundException(nameof(Quiz), request.QuizId);
        }

        var rows = _excelReader.ReadRows(request.FileContent);
        var errors = new List<string>();
        var successCount = 0;

        for (var i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            try
            {
                var text = ExcelRowReader.RequiredCell(row, 0, "QuestionText");
                var explanation = ExcelRowReader.Cell(row, 1);

                var options = new List<QuestionOption>();
                for (var optionIndex = 0; optionIndex < 4; optionIndex++)
                {
                    var textCol = 2 + optionIndex * 2;
                    var correctCol = textCol + 1;
                    var optionText = ExcelRowReader.Cell(row, textCol);
                    if (string.IsNullOrWhiteSpace(optionText))
                    {
                        continue;
                    }

                    options.Add(new QuestionOption { Text = optionText, IsCorrect = ExcelRowReader.ParseBool(row, correctCol) });
                }

                if (options.Count < 2)
                {
                    throw new InvalidOperationException("Ən azı 2 seçim olmalıdır.");
                }

                if (options.Count(o => o.IsCorrect) != 1)
                {
                    throw new InvalidOperationException("Düzgün olaraq yalnız bir seçim işarələnməlidir.");
                }

                _context.Questions.Add(new Question
                {
                    QuizId = request.QuizId,
                    Text = text,
                    Explanation = explanation,
                    Options = options,
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
            EntityType = "QuizQuestions",
            TotalRows = rows.Count,
            SuccessRows = successCount,
            FailedRows = errors.Count,
            ErrorDetails = errors.Count > 0 ? System.Text.Json.JsonSerializer.Serialize(errors) : null,
        });
        await _context.SaveChangesAsync(cancellationToken);

        return new ExcelImportResultDto(rows.Count, successCount, errors.Count, errors);
    }
}
