namespace QuestCraft.Application.Features.Admin.ExcelIO;

public record ExcelImportResultDto(int TotalRows, int SuccessRows, int FailedRows, List<string> Errors);
