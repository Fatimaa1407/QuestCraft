using ClosedXML.Excel;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Infrastructure.Excel;

public class ClosedXmlExportService : IExcelExportService
{
    public byte[] Export<T>(string sheetName, IReadOnlyList<string> headers, IEnumerable<T> rows, Func<T, object?[]> rowSelector)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(sheetName);

        for (var col = 0; col < headers.Count; col++)
        {
            worksheet.Cell(1, col + 1).Value = headers[col];
            worksheet.Cell(1, col + 1).Style.Font.Bold = true;
        }

        var rowIndex = 2;
        foreach (var row in rows)
        {
            var values = rowSelector(row);
            for (var col = 0; col < values.Length; col++)
            {
                worksheet.Cell(rowIndex, col + 1).Value = ToCellValue(values[col]);
            }

            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static XLCellValue ToCellValue(object? value) => value switch
    {
        null => string.Empty,
        bool b => b,
        int i => i,
        long l => l,
        double d => d,
        DateTime dt => dt,
        DateOnly d => d.ToDateTime(TimeOnly.MinValue),
        _ => value.ToString() ?? string.Empty,
    };
}
