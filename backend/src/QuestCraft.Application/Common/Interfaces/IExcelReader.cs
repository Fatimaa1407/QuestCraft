namespace QuestCraft.Application.Common.Interfaces;

public interface IExcelReader
{
    /// <summary>Reads all data rows (skipping the header row) as raw cell strings, sheet 1 only.</summary>
    IReadOnlyList<IReadOnlyList<string?>> ReadRows(byte[] fileContent);
}
