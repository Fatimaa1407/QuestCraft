using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Marketplace;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Marketplace;

public class ToggleWishlistCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, MarketplaceItem Item)> SeedAsync()
    {
        var db = InMemoryDbContextFactory.Create();

        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var user = new User { Username = "tester", FirstName = "T", LastName = "U", Email = "t@test.local", PasswordHash = "h", RoleId = role.Id };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var itemType = new MarketplaceItemType { Name = "Avatar" };
        db.MarketplaceItemTypes.Add(itemType);
        await db.SaveChangesAsync();
        var item = new MarketplaceItem { Name = "Test Avatar", ItemTypeId = itemType.Id, Price = 60, IsActive = true };
        db.MarketplaceItems.Add(item);
        await db.SaveChangesAsync();

        return (db, user, item);
    }

    [Fact]
    public async Task Handle_TogglingTwice_AddsThenRemoves()
    {
        var (db, user, item) = await SeedAsync();
        var handler = new ToggleWishlistCommandHandler(db, new FakeCurrentUserService { UserId = user.Id });

        var added = await handler.Handle(new ToggleWishlistCommand(item.Id), CancellationToken.None);
        Assert.True(added);
        Assert.True(await db.Wishlists.AnyAsync(w => w.UserId == user.Id && w.MarketplaceItemId == item.Id));

        var removed = await handler.Handle(new ToggleWishlistCommand(item.Id), CancellationToken.None);
        Assert.False(removed);
        Assert.False(await db.Wishlists.AnyAsync(w => w.UserId == user.Id && w.MarketplaceItemId == item.Id));
    }
}
