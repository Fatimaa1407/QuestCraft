using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class GameCompletionServiceTests
{
    private static async Task<(ApplicationDbContext Db, User User, Challenge Level1, Challenge Level2)> SeedAsync(int profileLevel)
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

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Level = profileLevel, Coins = 0 });

        var level1 = new Challenge { Title = "L1", Description = "d", StarterCode = "c", CategoryId = 1, DifficultyId = 1, RequiredLevel = 1, IsPublished = true };
        var level2 = new Challenge { Title = "L2", Description = "d", StarterCode = "c", CategoryId = 1, DifficultyId = 1, RequiredLevel = 2, IsPublished = true };
        db.Challenges.AddRange(level1, level2);

        var titleType = new MarketplaceItemType { Name = "Title" };
        var badgeType = new MarketplaceItemType { Name = "Badge" };
        db.MarketplaceItemTypes.AddRange(titleType, badgeType);
        await db.SaveChangesAsync();

        db.MarketplaceItems.AddRange(
            new MarketplaceItem { Name = GameCompletionService.TitleItemName, ItemTypeId = titleType.Id, Price = 0, IsActive = false },
            new MarketplaceItem { Name = GameCompletionService.BadgeItemName, ItemTypeId = badgeType.Id, Price = 0, IsActive = false, ImageUrl = "badge.svg" });
        await db.SaveChangesAsync();

        return (db, user, level1, level2);
    }

    private static async Task SolveAsync(ApplicationDbContext db, User user, Challenge challenge)
    {
        db.ChallengeSubmissions.Add(new ChallengeSubmission
        {
            UserId = user.Id,
            ChallengeId = challenge.Id,
            Verdict = SubmissionVerdict.Accepted,
            SourceCode = "x",
            SubmittedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task TryGrantCompletionRewardAsync_LevelBelowMax_ReturnsNull()
    {
        // maxAvailableLevel is 2 (Level2 challenge exists), profile is still at Level 1.
        var (db, user, _, _) = await SeedAsync(profileLevel: 1);
        var service = new GameCompletionService(db, new ContentCompletionService(db));

        var result = await service.TryGrantCompletionRewardAsync(user.Id, CancellationToken.None);

        Assert.Null(result);
        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(0, profile.Coins);
        Assert.Null(profile.GameCompletedAt);
    }

    [Fact]
    public async Task TryGrantCompletionRewardAsync_AtMaxLevelButIncomplete_ReturnsNull()
    {
        var (db, user, level1, _) = await SeedAsync(profileLevel: 2);
        // Only Level 1 solved — Level 2 (the max level) still has an unsolved challenge.
        await SolveAsync(db, user, level1);
        var service = new GameCompletionService(db, new ContentCompletionService(db));

        var result = await service.TryGrantCompletionRewardAsync(user.Id, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task TryGrantCompletionRewardAsync_AtMaxLevelAndComplete_GrantsRewardOnce()
    {
        var (db, user, level1, level2) = await SeedAsync(profileLevel: 2);
        await SolveAsync(db, user, level1);
        await SolveAsync(db, user, level2);
        var service = new GameCompletionService(db, new ContentCompletionService(db));

        var result = await service.TryGrantCompletionRewardAsync(user.Id, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.NotNull(result);
        Assert.Equal(2, result!.MaxLevel);
        Assert.Equal(GameCompletionService.CompletionBonusCoins, result.BonusCoins);
        Assert.Equal(GameCompletionService.TitleItemName, result.TitleText);
        Assert.Equal(GameCompletionService.BadgeItemName, result.BadgeName);

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(500, profile.Coins);
        Assert.NotNull(profile.GameCompletedAt);
        Assert.NotNull(profile.EquippedTitleId);
        Assert.NotNull(profile.EquippedBadgeId);

        var purchases = await db.Purchases.Where(p => p.UserId == user.Id).ToListAsync();
        Assert.Equal(2, purchases.Count);
        Assert.All(purchases, p => Assert.Equal(0, p.PricePaid));
    }

    [Fact]
    public async Task TryGrantCompletionRewardAsync_SecondCall_IsIdempotent()
    {
        var (db, user, level1, level2) = await SeedAsync(profileLevel: 2);
        await SolveAsync(db, user, level1);
        await SolveAsync(db, user, level2);
        var service = new GameCompletionService(db, new ContentCompletionService(db));

        var first = await service.TryGrantCompletionRewardAsync(user.Id, CancellationToken.None);
        await db.SaveChangesAsync();
        var second = await service.TryGrantCompletionRewardAsync(user.Id, CancellationToken.None);
        await db.SaveChangesAsync();

        Assert.NotNull(first);
        Assert.Null(second);

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(500, profile.Coins);
        var purchases = await db.Purchases.Where(p => p.UserId == user.Id).ToListAsync();
        Assert.Equal(2, purchases.Count);
    }
}
