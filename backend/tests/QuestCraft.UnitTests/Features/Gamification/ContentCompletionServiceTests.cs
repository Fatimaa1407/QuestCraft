using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class ContentCompletionServiceTests
{
    private static async Task<(ApplicationDbContext Db, User User)> SeedUserAsync()
    {
        var db = InMemoryDbContextFactory.Create();

        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User
        {
            Username = "tester",
            FirstName = "Test",
            LastName = "User",
            Email = "tester@test.local",
            PasswordHash = "hash",
            RoleId = role.Id,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Level = 1 });
        await db.SaveChangesAsync();

        return (db, user);
    }

    private static Challenge MakeChallenge(int requiredLevel, bool isBattleOnly = false, bool isDailyPuzzle = false) => new()
    {
        Title = $"Challenge L{requiredLevel}",
        Description = "Desc",
        StarterCode = "// code",
        CategoryId = 1,
        DifficultyId = 1,
        RequiredLevel = requiredLevel,
        IsPublished = true,
        IsBattleOnly = isBattleOnly,
        IsDailyPuzzle = isDailyPuzzle,
    };

    private static Quiz MakeQuiz(int requiredLevel) => new()
    {
        Title = $"Quiz L{requiredLevel}",
        RequiredLevel = requiredLevel,
        IsPublished = true,
    };

    [Fact]
    public async Task GetMaxAvailableLevelAsync_NoPublishedContent_ReturnsOne()
    {
        var (db, _) = await SeedUserAsync();
        var service = new ContentCompletionService(db);

        var maxLevel = await service.GetMaxAvailableLevelAsync(CancellationToken.None);

        Assert.Equal(1, maxLevel);
    }

    [Fact]
    public async Task GetMaxAvailableLevelAsync_ReturnsHighestPublishedRequiredLevelAcrossChallengesAndQuizzes()
    {
        var (db, _) = await SeedUserAsync();
        db.Challenges.AddRange(MakeChallenge(1), MakeChallenge(2));
        db.Quizzes.Add(MakeQuiz(3));
        await db.SaveChangesAsync();
        var service = new ContentCompletionService(db);

        var maxLevel = await service.GetMaxAvailableLevelAsync(CancellationToken.None);

        Assert.Equal(3, maxLevel);
    }

    [Fact]
    public async Task GetMaxAvailableLevelAsync_IgnoresBattleOnlyDailyPuzzleAndUnpublishedContent()
    {
        var (db, _) = await SeedUserAsync();
        db.Challenges.AddRange(
            MakeChallenge(1),
            MakeChallenge(10, isBattleOnly: true),
            MakeChallenge(10, isDailyPuzzle: true),
            new Challenge { Title = "Unpublished", Description = "Desc", StarterCode = "// code", CategoryId = 1, DifficultyId = 1, RequiredLevel = 10, IsPublished = false });
        await db.SaveChangesAsync();
        var service = new ContentCompletionService(db);

        var maxLevel = await service.GetMaxAvailableLevelAsync(CancellationToken.None);

        Assert.Equal(1, maxLevel);
    }

    [Fact]
    public async Task CalculateUnlockedLevelAsync_CapsAtMaxAvailableLevel_EvenWhenAllContentComplete()
    {
        var (db, user) = await SeedUserAsync();
        var level1 = MakeChallenge(1);
        var level2 = MakeChallenge(2);
        db.Challenges.AddRange(level1, level2);
        await db.SaveChangesAsync();

        db.ChallengeSubmissions.AddRange(
            new ChallengeSubmission { UserId = user.Id, ChallengeId = level1.Id, Verdict = SubmissionVerdict.Accepted, SourceCode = "x", SubmittedAt = DateTime.UtcNow },
            new ChallengeSubmission { UserId = user.Id, ChallengeId = level2.Id, Verdict = SubmissionVerdict.Accepted, SourceCode = "x", SubmittedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);

        var unlockedLevel = await service.CalculateUnlockedLevelAsync(user.Id, CancellationToken.None);

        // Both published levels (max = 2) are fully solved, but the level must never advance past the
        // game's current content ceiling — this is exactly the "no Level 13" requirement.
        Assert.Equal(2, unlockedLevel);
    }

    [Fact]
    public async Task CalculateUnlockedLevelAsync_StopsAtFirstIncompleteLevel()
    {
        var (db, user) = await SeedUserAsync();
        var level1 = MakeChallenge(1);
        var level2 = MakeChallenge(2);
        db.Challenges.AddRange(level1, level2);
        await db.SaveChangesAsync();

        // Only level 1 solved — level 2 exists but is untouched.
        db.ChallengeSubmissions.Add(
            new ChallengeSubmission { UserId = user.Id, ChallengeId = level1.Id, Verdict = SubmissionVerdict.Accepted, SourceCode = "x", SubmittedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);

        var unlockedLevel = await service.CalculateUnlockedLevelAsync(user.Id, CancellationToken.None);

        Assert.Equal(2, unlockedLevel);
    }

    [Fact]
    public async Task GetLevelCompletionAsync_AllAttemptedButLowQuizScores_IsNotComplete()
    {
        // Closes the "write whatever comes to mind" loophole: every quiz at the level has been
        // attempted (so the raw completed-count would look done), but scores are far below the
        // 70% bar, so the level must not count as complete.
        var (db, user) = await SeedUserAsync();
        var quiz = MakeQuiz(1);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        db.QuizAttempts.Add(new QuizAttempt { UserId = user.Id, QuizId = quiz.Id, Score = 1, TotalQuestions = 10, CompletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);
        var completion = await service.GetLevelCompletionAsync(user.Id, 1, CancellationToken.None);

        Assert.Equal(1, completion.QuizzesCompleted);
        Assert.Equal(1, completion.QuizzesTotal);
        Assert.Equal(10.0, completion.AverageScorePercent);
        Assert.False(completion.IsComplete);
    }

    [Fact]
    public async Task GetLevelCompletionAsync_RetakingAQuiz_UsesBestScoreNotLatest()
    {
        var (db, user) = await SeedUserAsync();
        var quiz = MakeQuiz(1);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        // First attempt scores low, a later retake scores perfectly — the user's best effort should
        // count, not their first (or most recent) attempt.
        db.QuizAttempts.AddRange(
            new QuizAttempt { UserId = user.Id, QuizId = quiz.Id, Score = 2, TotalQuestions = 10, CompletedAt = DateTime.UtcNow.AddMinutes(-10) },
            new QuizAttempt { UserId = user.Id, QuizId = quiz.Id, Score = 10, TotalQuestions = 10, CompletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);
        var completion = await service.GetLevelCompletionAsync(user.Id, 1, CancellationToken.None);

        Assert.Equal(100.0, completion.AverageScorePercent);
        Assert.True(completion.IsComplete);
    }

    [Fact]
    public async Task GetLevelCompletionAsync_AllAttemptedWithPassingAverage_IsComplete()
    {
        var (db, user) = await SeedUserAsync();
        var challenge = MakeChallenge(1);
        var quiz = MakeQuiz(1);
        db.Challenges.Add(challenge);
        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        db.ChallengeSubmissions.Add(
            new ChallengeSubmission { UserId = user.Id, ChallengeId = challenge.Id, Verdict = SubmissionVerdict.Accepted, SourceCode = "x", SubmittedAt = DateTime.UtcNow });
        db.QuizAttempts.Add(new QuizAttempt { UserId = user.Id, QuizId = quiz.Id, Score = 8, TotalQuestions = 10, CompletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);
        var completion = await service.GetLevelCompletionAsync(user.Id, 1, CancellationToken.None);

        // Challenge contributes 100%, quiz contributes 80% -> average 90%, above the 70% bar.
        Assert.Equal(90.0, completion.AverageScorePercent);
        Assert.True(completion.IsComplete);
    }

    [Fact]
    public async Task CalculateUnlockedLevelAsync_LowAverageAtLevel1_NeverAdvances()
    {
        var (db, user) = await SeedUserAsync();
        var level1Quiz = MakeQuiz(1);
        var level2Challenge = MakeChallenge(2);
        db.Quizzes.Add(level1Quiz);
        db.Challenges.Add(level2Challenge);
        await db.SaveChangesAsync();

        // Level 1's only item is "attempted" but scored far below the passing average.
        db.QuizAttempts.Add(new QuizAttempt { UserId = user.Id, QuizId = level1Quiz.Id, Score = 0, TotalQuestions = 10, CompletedAt = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var service = new ContentCompletionService(db);
        var unlockedLevel = await service.CalculateUnlockedLevelAsync(user.Id, CancellationToken.None);

        Assert.Equal(1, unlockedLevel);
    }
}
