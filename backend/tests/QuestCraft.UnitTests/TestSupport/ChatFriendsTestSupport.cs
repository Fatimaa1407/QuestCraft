using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;

namespace QuestCraft.UnitTests.TestSupport;

public static class ChatFriendsTestSupport
{
    public static async Task<(ApplicationDbContext Db, User UserA, User UserB)> SeedTwoUsersAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var userA = await BattleTestSupport.CreateUserAsync(db, "userA", role.Id);
        var userB = await BattleTestSupport.CreateUserAsync(db, "userB", role.Id);

        return (db, userA, userB);
    }

    public static async Task MakeFriendsAsync(ApplicationDbContext db, int userAId, int userBId)
    {
        db.FriendRequests.Add(new FriendRequest { RequesterId = userAId, AddresseeId = userBId, Status = FriendRequestStatus.Accepted });
        await db.SaveChangesAsync();
    }
}
