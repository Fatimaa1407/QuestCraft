using QuestCraft.Domain.Common;

namespace QuestCraft.Domain.Entities;

public class Challenge : BaseEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public int TimeLimitMs { get; set; } = 2000;
    public int MemoryLimitMb { get; set; } = 256;
    public int XpReward { get; set; }
    public int CoinReward { get; set; }
    public string StarterCode { get; set; } = default!;
    public string? Constraints { get; set; }
    public string? InputFormat { get; set; }
    public string? OutputFormat { get; set; }
    public string? SampleInput { get; set; }
    public string? SampleOutput { get; set; }
    public string? Hint { get; set; }
    public bool IsPublished { get; set; }
    public int RequiredLevel { get; set; } = 1;

    // English translations — nullable; falls back to the Azerbaijani field above when missing.
    public string? TitleEn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? ConstraintsEn { get; set; }
    public string? InputFormatEn { get; set; }
    public string? OutputFormatEn { get; set; }
    public string? HintEn { get; set; }
    public string? StarterCodeEn { get; set; }

    public int CategoryId { get; set; }
    public ChallengeCategory Category { get; set; } = default!;

    public int DifficultyId { get; set; }
    public ChallengeDifficulty Difficulty { get; set; } = default!;

    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<HiddenTestCase> HiddenTestCases { get; set; } = new List<HiddenTestCase>();
    public ICollection<ChallengeSubmission> Submissions { get; set; } = new List<ChallengeSubmission>();
    public ICollection<ChallengeHint> UnlockedHints { get; set; } = new List<ChallengeHint>();
}
