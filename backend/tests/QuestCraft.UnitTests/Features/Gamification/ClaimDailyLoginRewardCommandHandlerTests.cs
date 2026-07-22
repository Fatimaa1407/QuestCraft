using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class ClaimDailyLoginRewardCommandHandlerTests
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

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Coins = 0, Xp = 0 });
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_FirstClaimOfDay_GrantsRewardAndSetsClaimDate()
    {
        var (db, user) = await SeedUserAsync();
        var handler = new ClaimDailyLoginRewardCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new ClaimDailyLoginRewardCommand(), CancellationToken.None);

        Assert.False(result.AlreadyClaimed);
        Assert.True(result.CoinsAwarded > 0);
        Assert.True(result.XpAwarded > 0);
        Assert.Equal(result.CoinsAwarded, result.NewCoinsTotal);
        Assert.Equal(result.XpAwarded, result.NewXpTotal);

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(DateOnly.FromDateTime(DateTime.UtcNow), profile.LastLoginRewardClaimedAt);
    }

    [Fact]
    public async Task Handle_SecondClaimSameDay_ReturnsAlreadyClaimedWithoutGrantingAgain()
    {
        var (db, user) = await SeedUserAsync();
        var handler = new ClaimDailyLoginRewardCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var first = await handler.Handle(new ClaimDailyLoginRewardCommand(), CancellationToken.None);
        var second = await handler.Handle(new ClaimDailyLoginRewardCommand(), CancellationToken.None);

        Assert.True(second.AlreadyClaimed);
        Assert.Equal(0, second.CoinsAwarded);
        Assert.Equal(0, second.XpAwarded);
        // Totals reflect the first claim only — the second call granted nothing further.
        Assert.Equal(first.NewCoinsTotal, second.NewCoinsTotal);
        Assert.Equal(first.NewXpTotal, second.NewXpTotal);
    }
}
