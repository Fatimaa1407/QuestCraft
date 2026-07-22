using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Marketplace;

public class PurchaseItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, MarketplaceItem Item)> SeedAsync(int coins, int price)
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

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Coins = coins });

        var itemType = new MarketplaceItemType { Name = "Avatar" };
        db.MarketplaceItemTypes.Add(itemType);
        await db.SaveChangesAsync();

        var item = new MarketplaceItem { Name = "Test Item", ItemTypeId = itemType.Id, Price = price, IsActive = true };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();

        return (db, user, item);
    }

    [Fact]
    public async Task Handle_SuccessfulPurchase_DeductsCoinsAndCreatesPurchase()
    {
        var (db, user, item) = await SeedAsync(coins: 100, price: 60);
        var handler = new PurchaseItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new PurchaseItemCommand(item.Id), CancellationToken.None);

        Assert.Equal(40, result.RemainingCoins);
        Assert.Equal(item.Id, result.MarketplaceItemId);
        Assert.True(await db.Purchases.AnyAsync(p => p.UserId == user.Id && p.MarketplaceItemId == item.Id));
    }

    [Fact]
    public async Task Handle_InsufficientCoins_ThrowsBadRequest()
    {
        var (db, user, item) = await SeedAsync(coins: 10, price: 60);
        var handler = new PurchaseItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new PurchaseItemCommand(item.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyOwned_ThrowsConflict()
    {
        var (db, user, item) = await SeedAsync(coins: 500, price: 60);
        var handler = new PurchaseItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        await handler.Handle(new PurchaseItemCommand(item.Id), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new PurchaseItemCommand(item.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsNotFound()
    {
        var (db, user, _) = await SeedAsync(coins: 500, price: 60);
        var handler = new PurchaseItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new PurchaseItemCommand(999_999), CancellationToken.None));
    }
}
