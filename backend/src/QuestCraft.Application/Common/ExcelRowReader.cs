namespace QuestCraft.Application.Common;

// Small helpers shared by every Excel import command handler for pulling typed values out of a raw row.
public static class ExcelRowReader
{
    public static string? Cell(IReadOnlyList<string?> row, int index) => index < row.Count ? row[index] : null;

    public static string RequiredCell(IReadOnlyList<string?> row, int index, string fieldName)
    {
        var value = Cell(row, index);
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"\"{fieldName}\" boş ola bilməz.");
        }

        return value;
    }

    public static int ParseInt(IReadOnlyList<string?> row, int index, int fallback) =>
        int.TryParse(Cell(row, index), out var value) ? value : fallback;

    public static bool ParseBool(IReadOnlyList<string?> row, int index)
    {
        var cell = Cell(row, index);
        return cell is not null && (cell.Equals("true", StringComparison.OrdinalIgnoreCase) || cell == "1");
    }
}
