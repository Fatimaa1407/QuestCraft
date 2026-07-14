namespace QuestCraft.Application.Common;

/// <summary>
/// Picks the English translation of a content field when requested and available, falling back to the
/// Azerbaijani (primary) value otherwise — content rows aren't required to have an English translation yet.
/// </summary>
public static class LocalizationHelper
{
    public static string Pick(string az, string? en, bool isEnglish) =>
        isEnglish && !string.IsNullOrWhiteSpace(en) ? en : az;

    public static string? PickNullable(string? az, string? en, bool isEnglish) =>
        isEnglish && !string.IsNullOrWhiteSpace(en) ? en : az;
}
