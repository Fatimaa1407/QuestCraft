namespace QuestCraft.Application.Common.Interfaces;

public interface IExcelExportService
{
    byte[] Export<T>(string sheetName, IReadOnlyList<string> headers, IEnumerable<T> rows, Func<T, object?[]> rowSelector);
}
