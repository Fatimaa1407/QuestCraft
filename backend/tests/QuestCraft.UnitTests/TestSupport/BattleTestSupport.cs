using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;

namespace QuestCraft.UnitTests.TestSupport;

public static class BattleTestSupport
{
    public static async Task<User> CreateUserAsync(ApplicationDbContext db, string username, int roleId)
    {
        var user = new User
        {
            Username = username,
            FirstName = username,
            LastName = "Test",
            Email = $"{username}@test.local",
            PasswordHash = "hash",
            RoleId = roleId,
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserProfiles.Add(new UserProfile { UserId = user.Id });
        db.UserStatistics.Add(new UserStatistics { UserId = user.Id });
        await db.SaveChangesAsync();

        return user;
    }

    // A single-test-case, battle-pool-eligible challenge — enough for FakeCodeExecutionEngine to
    // report either a full pass (AllPass = true) or a full fail (AllPass = false).
    public static async Task<Challenge> CreateBattleChallengeAsync(ApplicationDbContext db, int xpReward = 40, int coinReward = 15)
    {
        var category = new ChallengeCategory { Name = $"Cat-{Guid.NewGuid():N}" };
        var difficulty = new ChallengeDifficulty { Name = $"Diff-{Guid.NewGuid():N}" };
        db.ChallengeCategories.Add(category);
        db.ChallengeDifficulties.Add(difficulty);
        await db.SaveChangesAsync();

        var challenge = new Challenge
        {
            Title = "Battle Challenge",
            Description = "Solve it.",
            StarterCode = "// code",
            IsPublished = true,
            IsBattleOnly = true,
            XpReward = xpReward,
            CoinReward = coinReward,
            CategoryId = category.Id,
            DifficultyId = difficulty.Id,
        };
        db.Challenges.Add(challenge);
        await db.SaveChangesAsync();

        db.TestCases.Add(new TestCase { ChallengeId = challenge.Id, Input = "1", ExpectedOutput = "1", OrderIndex = 0 });
        await db.SaveChangesAsync();

        return challenge;
    }
}
