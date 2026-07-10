using ClosedXML.Excel;
using QuestCraft.Application.Common.Interfaces;

namespace QuestCraft.Infrastructure.Excel;

public class ClosedXmlExcelReader : IExcelReader
{
    public IReadOnlyList<IReadOnlyList<string?>> ReadRows(byte[] fileContent)
    {
        using var stream = new MemoryStream(fileContent);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        var rows = new List<IReadOnlyList<string?>>();
        var usedRange = worksheet.RangeUsed();
        if (usedRange is null)
        {
            return rows;
        }

        var lastColumn = usedRange.LastColumn().ColumnNumber();

        // Row 1 is the header — data starts at row 2.
        foreach (var row in worksheet.RowsUsed().Skip(1))
        {
            var cells = new List<string?>();
            for (var col = 1; col <= lastColumn; col++)
            {
                var cell = row.Cell(col);
                cells.Add(cell.IsEmpty() ? null : cell.GetValue<string>());
            }

            rows.Add(cells);
        }

        return rows;
    }
}
