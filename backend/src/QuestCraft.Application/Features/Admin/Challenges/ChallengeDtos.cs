namespace QuestCraft.Application.Features.Admin.Challenges;

public record ChallengeListItemDto(
    int Id,
    string Title,
    string Category,
    string Difficulty,
    int XpReward,
    int CoinReward,
    bool IsPublished,
    int RequiredLevel,
    bool IsLocked,
    string? Tags = null,
    bool IsBattleOnly = false);

public record TestCaseDto(int Id, string Input, string ExpectedOutput, int OrderIndex);

public record HiddenTestCaseDto(int Id, string Input, string ExpectedOutput, int OrderIndex, int Weight);

public record ChallengeDetailDto(
    int Id,
    string Title,
    string Description,
    int CategoryId,
    string Category,
    int DifficultyId,
    string Difficulty,
    int TimeLimitMs,
    int MemoryLimitMb,
    int XpReward,
    int CoinReward,
    string StarterCode,
    string? Constraints,
    string? InputFormat,
    string? OutputFormat,
    string? SampleInput,
    string? SampleOutput,
    bool IsPublished,
    int RequiredLevel,
    List<TestCaseDto> TestCases,
    List<HiddenTestCaseDto>? HiddenTestCases,
    bool IsAlreadySolved,
    // Raw (unresolved) English variants, for the admin edit form only — every other field above is
    // already localized to the current viewer's language via LocalizationHelper.
    string? TitleEn = null,
    string? DescriptionEn = null,
    string? ConstraintsEn = null,
    string? InputFormatEn = null,
    string? OutputFormatEn = null,
    string? StarterCodeEn = null,
    string? Tags = null,
    bool IsBattleOnly = false);
