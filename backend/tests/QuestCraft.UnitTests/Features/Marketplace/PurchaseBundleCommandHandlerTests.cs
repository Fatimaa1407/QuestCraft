using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Marketplace;

public class PurchaseBundleCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, MarketplaceBundle Bundle, MarketplaceItem ItemA, MarketplaceItem ItemB)> SeedAsync(int coins)
    {
        var db = InMemoryDbContextFactory.Create();

        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User { Username = "tester", FirstName = "T", LastName = "U", Email = "t@test.local", PasswordHash = "h", RoleId = role.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.UserProfiles.Add(new UserProfile { UserId = user.Id, Coins = coins });

        var avatarType = new MarketplaceItemType { Name = "Avatar" };
        var badgeType = new MarketplaceItemType { Name = "Badge" };
        db.MarketplaceItemTypes.AddRange(avatarType, badgeType);
        await db.SaveChangesAsync();

        var itemA = new MarketplaceItem { Name = "Bundle Avatar", ItemTypeId = avatarType.Id, Price = 60, IsActive = true };
        var itemB = new MarketplaceItem { Name = "Bundle Badge", ItemTypeId = badgeType.Id, Price = 40, IsActive = true };
        db.MarketplaceItems.AddRange(itemA, itemB);
        await db.SaveChangesAsync();

        var bundle = new MarketplaceBundle { Name = "Test Bundle", BundlePrice = 80, IsActive = true };
        bundle.Items.Add(new MarketplaceBundleItem { MarketplaceItem = itemA });
        bundle.Items.Add(new MarketplaceBundleItem { MarketplaceItem = itemB });
        db.MarketplaceBundles.Add(bundle);
        await db.SaveChangesAsync();

        return (db, user, bundle, itemA, itemB);
    }

    [Fact]
    public async Task Handle_SuccessfulPurchase_GrantsAllItemsAndAutoEquips()
    {
        var (db, user, bundle, itemA, itemB) = await SeedAsync(coins: 100);
        var handler = new PurchaseBundleCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var result = await handler.Handle(new PurchaseBundleCommand(bundle.Id), CancellationToken.None);

        Assert.Equal(20, result.RemainingCoins);
        Assert.Equal(2, result.GrantedItemNames.Count);
        Assert.True(await db.Purchases.AnyAsync(p => p.UserId == user.Id && p.MarketplaceItemId == itemA.Id && p.PricePaid == 0));
        Assert.True(await db.Purchases.AnyAsync(p => p.UserId == user.Id && p.MarketplaceItemId == itemB.Id && p.PricePaid == 0));

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        Assert.Equal(itemA.Id, profile.EquippedAvatarId);
        Assert.Equal(itemB.Id, profile.EquippedBadgeId);
    }

    [Fact]
    public async Task Handle_PartiallyOwned_OnlyGrantsMissingItems()
    {
        var (db, user, bundle, itemA, _) = await SeedAsync(coins: 100);
        db.Purchases.Add(new Purchase { UserId = user.Id, MarketplaceItemId = itemA.Id, PricePaid = itemA.Price });
        await db.SaveChangesAsync();

        var handler = new PurchaseBundleCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });
        var result = await handler.Handle(new PurchaseBundleCommand(bundle.Id), CancellationToken.None);

        Assert.Single(result.GrantedItemNames);
        Assert.Equal(1, await db.Purchases.CountAsync(p => p.UserId == user.Id && p.MarketplaceItemId == itemA.Id));
    }

    [Fact]
    public async Task Handle_AlreadyFullyOwned_ThrowsConflict()
    {
        var (db, user, bundle, itemA, itemB) = await SeedAsync(coins: 200);
        db.Purchases.Add(new Purchase { UserId = user.Id, MarketplaceItemId = itemA.Id, PricePaid = itemA.Price });
        db.Purchases.Add(new Purchase { UserId = user.Id, MarketplaceItemId = itemB.Id, PricePaid = itemB.Price });
        await db.SaveChangesAsync();

        var handler = new PurchaseBundleCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<ConflictException>(
            () => handler.Handle(new PurchaseBundleCommand(bundle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_InsufficientCoins_ThrowsBadRequest()
    {
        var (db, user, bundle, _, _) = await SeedAsync(coins: 10);
        var handler = new PurchaseBundleCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new PurchaseBundleCommand(bundle.Id), CancellationToken.None));
    }
}
