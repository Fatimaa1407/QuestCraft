using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class PinAchievementCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, List<Achievement> Achievements)> SeedUnlockedAchievementsAsync(int count)
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

        var achievements = new List<Achievement>();
        for (var i = 0; i < count; i++)
        {
            var achievement = new Achievement { Name = $"Achievement {i}", Description = "Test" };
            db.Achievements.Add(achievement);
            achievements.Add(achievement);
        }
        await db.SaveChangesAsync();

        foreach (var achievement in achievements)
        {
            db.UserAchievements.Add(new UserAchievement { UserId = user.Id, AchievementId = achievement.Id });
        }
        await db.SaveChangesAsync();

        return (db, user, achievements);
    }

    [Fact]
    public async Task Handle_UnlockedAchievement_PinsSuccessfully()
    {
        var (db, user, achievements) = await SeedUnlockedAchievementsAsync(1);
        var handler = new PinAchievementCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await handler.Handle(new PinAchievementCommand(achievements[0].Id), CancellationToken.None);

        var userAchievement = await db.UserAchievements.FirstAsync(ua => ua.UserId == user.Id && ua.AchievementId == achievements[0].Id);
        Assert.True(userAchievement.IsPinned);
    }

    [Fact]
    public async Task Handle_NotUnlocked_ThrowsBadRequest()
    {
        var (db, user, _) = await SeedUnlockedAchievementsAsync(0);
        var lockedAchievement = new Achievement { Name = "Locked", Description = "Test" };
        db.Achievements.Add(lockedAchievement);
        await db.SaveChangesAsync();

        var handler = new PinAchievementCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new PinAchievementCommand(lockedAchievement.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyThreePinned_ThrowsConflict()
    {
        var (db, user, achievements) = await SeedUnlockedAchievementsAsync(4);
        var handler = new PinAchievementCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await handler.Handle(new PinAchievementCommand(achievements[0].Id), CancellationToken.None);
        await handler.Handle(new PinAchievementCommand(achievements[1].Id), CancellationToken.None);
        await handler.Handle(new PinAchievementCommand(achievements[2].Id), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new PinAchievementCommand(achievements[3].Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Unpin_ClearsPinnedFlag()
    {
        var (db, user, achievements) = await SeedUnlockedAchievementsAsync(1);
        var pinHandler = new PinAchievementCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var unpinHandler = new UnpinAchievementCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await pinHandler.Handle(new PinAchievementCommand(achievements[0].Id), CancellationToken.None);
        await unpinHandler.Handle(new UnpinAchievementCommand(achievements[0].Id), CancellationToken.None);

        var userAchievement = await db.UserAchievements.FirstAsync(ua => ua.UserId == user.Id && ua.AchievementId == achievements[0].Id);
        Assert.False(userAchievement.IsPinned);
    }
}
