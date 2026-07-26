using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Marketplace;

public class OpenMysteryBoxCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User)> SeedUserAsync(int coins)
    {
        var db = InMemoryDbContextFactory.Create();

        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User { Username = "tester", FirstName = "T", LastName = "U", Email = "t@test.local", PasswordHash = "h", RoleId = role.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Coins = coins });
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_EmptyPool_ReturnsNothingLeftWithoutCharging()
    {
        var (db, user) = await SeedUserAsync(coins: 500);
        var handler = new OpenMysteryBoxCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new OpenMysteryBoxCommand(), CancellationToken.None);

        Assert.True(result.NothingLeft);
        Assert.Null(result.ItemId);
        Assert.Equal(500, result.RemainingCoins);
        Assert.False(await db.Purchases.AnyAsync(p => p.UserId == user.Id));
    }

    [Fact]
    public async Task Handle_ExcludesOwnedAndStreakFreezeItems()
    {
        var (db, user) = await SeedUserAsync(coins: 500);

        var avatarType = new MarketplaceItemType { Name = "Avatar" };
        var streakType = new MarketplaceItemType { Name = "StreakFreeze" };
        db.MarketplaceItemTypes.AddRange(avatarType, streakType);
        await db.SaveChangesAsync();

        var owned = new MarketplaceItem { Name = "Owned Avatar", ItemTypeId = avatarType.Id, Price = 60, IsActive = true };
        var streakFreeze = new MarketplaceItem { Name = "Freeze", ItemTypeId = streakType.Id, Price = 120, IsActive = true };
        db.MarketplaceItems.AddRange(owned, streakFreeze);
        await db.SaveChangesAsync();
        db.Purchases.Add(new Purchase { UserId = user.Id, MarketplaceItemId = owned.Id, PricePaid = owned.Price });
        await db.SaveChangesAsync();

        var handler = new OpenMysteryBoxCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var result = await handler.Handle(new OpenMysteryBoxCommand(), CancellationToken.None);

        Assert.True(result.NothingLeft);
    }

    [Fact]
    public async Task Handle_GrantsUnownedCosmeticAndCharges100Coins()
    {
        var (db, user) = await SeedUserAsync(coins: 500);

        var avatarType = new MarketplaceItemType { Name = "Avatar" };
        db.MarketplaceItemTypes.Add(avatarType);
        await db.SaveChangesAsync();
        var item = new MarketplaceItem { Name = "Grantable Avatar", ItemTypeId = avatarType.Id, Price = 60, IsActive = true };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();

        var handler = new OpenMysteryBoxCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var result = await handler.Handle(new OpenMysteryBoxCommand(), CancellationToken.None);

        Assert.False(result.NothingLeft);
        Assert.Equal(item.Id, result.ItemId);
        Assert.Equal(400, result.RemainingCoins);
        Assert.True(result.AutoEquipped);
        Assert.True(await db.Purchases.AnyAsync(p => p.UserId == user.Id && p.MarketplaceItemId == item.Id && p.PricePaid == 100));
    }

    [Fact]
    public async Task Handle_InsufficientCoins_ThrowsBadRequest()
    {
        var (db, user) = await SeedUserAsync(coins: 10);
        var avatarType = new MarketplaceItemType { Name = "Avatar" };
        db.MarketplaceItemTypes.Add(avatarType);
        await db.SaveChangesAsync();
        db.MarketplaceItems.Add(new MarketplaceItem { Name = "Expensive Avatar", ItemTypeId = avatarType.Id, Price = 60, IsActive = true });
        await db.SaveChangesAsync();

        var handler = new OpenMysteryBoxCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new OpenMysteryBoxCommand(), CancellationToken.None));
    }
}
