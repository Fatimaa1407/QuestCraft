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
    public bool IsPublished { get; set; }
    public int RequiredLevel { get; set; } = 1;

    // Comma-separated, e.g. "linq,collections,beginner" — simple free-text tagging rather than a
    // normalized many-to-many table, since search/filter is the only consumer and volume is small.
    public string? Tags { get; set; }

    // Battle-pool challenges are a separate set from the leveled practice list: excluded from
    // GetChallengesQuery entirely (never appear at any level) and drawn from at random when a
    // Battle room/duel is created, so no participant could have already seen or solved it.
    public bool IsBattleOnly { get; set; }

    // Daily-puzzle-pool challenges are also excluded from GetChallengesQuery. Which one is "today's"
    // puzzle is never stored — it's derived deterministically from the date (see GetDailyPuzzleQuery),
    // so this flag only marks pool membership, not the currently-active pick.
    public bool IsDailyPuzzle { get; set; }

    // English translations — nullable; falls back to the Azerbaijani field above when missing.
    public string? TitleEn { get; set; }
    public string? DescriptionEn { get; set; }
    public string? ConstraintsEn { get; set; }
    public string? InputFormatEn { get; set; }
    public string? OutputFormatEn { get; set; }
    public string? StarterCodeEn { get; set; }

    public int CategoryId { get; set; }
    public ChallengeCategory Category { get; set; } = default!;

    public int DifficultyId { get; set; }
    public ChallengeDifficulty Difficulty { get; set; } = default!;

    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
    public ICollection<HiddenTestCase> HiddenTestCases { get; set; } = new List<HiddenTestCase>();
    public ICollection<ChallengeSubmission> Submissions { get; set; } = new List<ChallengeSubmission>();
}
