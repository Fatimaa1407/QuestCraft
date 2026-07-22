using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Marketplace;

public class EquipItemCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, MarketplaceItem Item)> SeedOwnedItemAsync(string itemTypeName)
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

        db.UserProfiles.Add(new UserProfile { UserId = user.Id });

        var itemType = new MarketplaceItemType { Name = itemTypeName };
        db.MarketplaceItemTypes.Add(itemType);
        await db.SaveChangesAsync();

        var item = new MarketplaceItem { Name = $"Test {itemTypeName}", ItemTypeId = itemType.Id, Price = 10, IsActive = true };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();

        db.Purchases.Add(new Purchase { UserId = user.Id, MarketplaceItemId = item.Id, PricePaid = item.Price });
        await db.SaveChangesAsync();

        return (db, user, item);
    }

    [Theory]
    [InlineData("Avatar")]
    [InlineData("ProfileFrame")]
    [InlineData("ProfileBanner")]
    [InlineData("Title")]
    [InlineData("Badge")]
    [InlineData("Theme")]
    public async Task Handle_OwnedItem_SetsCorrectProfileSlot(string itemTypeName)
    {
        var (db, user, item) = await SeedOwnedItemAsync(itemTypeName);
        var handler = new EquipItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await handler.Handle(new EquipItemCommand(item.Id), CancellationToken.None);

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == user.Id);
        var equippedId = itemTypeName switch
        {
            "Avatar" => profile.EquippedAvatarId,
            "ProfileFrame" => profile.EquippedFrameId,
            "ProfileBanner" => profile.EquippedBannerId,
            "Title" => profile.EquippedTitleId,
            "Badge" => profile.EquippedBadgeId,
            "Theme" => profile.EquippedThemeId,
            _ => throw new InvalidOperationException(),
        };
        Assert.Equal(item.Id, equippedId);
    }

    [Fact]
    public async Task Handle_NotOwned_ThrowsBadRequest()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var user = new User { Username = "t2", FirstName = "T", LastName = "T", Email = "t2@test.local", PasswordHash = "h", RoleId = role.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        db.UserProfiles.Add(new UserProfile { UserId = user.Id });
        var itemType = new MarketplaceItemType { Name = "Avatar" };
        db.MarketplaceItemTypes.Add(itemType);
        await db.SaveChangesAsync();
        var item = new MarketplaceItem { Name = "Unowned", ItemTypeId = itemType.Id, Price = 10 };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();

        var handler = new EquipItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new EquipItemCommand(item.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_UnsupportedItemType_ThrowsBadRequest()
    {
        var (db, user, item) = await SeedOwnedItemAsync("Hint");
        var handler = new EquipItemCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        await Assert.ThrowsAsync<BadRequestException>(
            () => handler.Handle(new EquipItemCommand(item.Id), CancellationToken.None));
    }
}
