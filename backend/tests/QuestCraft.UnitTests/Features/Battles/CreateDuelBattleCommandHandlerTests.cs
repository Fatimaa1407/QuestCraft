using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class CreateDuelBattleCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User Me, User Friend, User Stranger)> SeedAsync()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var me = await BattleTestSupport.CreateUserAsync(db, "me", role.Id);
        var friend = await BattleTestSupport.CreateUserAsync(db, "friend", role.Id);
        var stranger = await BattleTestSupport.CreateUserAsync(db, "stranger", role.Id);
        await BattleTestSupport.CreateBattleChallengeAsync(db);

        db.FriendRequests.Add(new FriendRequest { RequesterId = me.Id, AddresseeId = friend.Id, Status = FriendRequestStatus.Accepted });
        await db.SaveChangesAsync();

        return (db, me, friend, stranger);
    }

    [Fact]
    public async Task Handle_Friend_CreatesWaitingDuel()
    {
        var (db, me, friend, _) = await SeedAsync();
        var handler = new CreateDuelBattleCommandHandler(db, new FakeCurrentUserService { UserId = me.Id }, new FakeRealtimeNotifier());

        var dto = await handler.Handle(new CreateDuelBattleCommand(friend.Id), CancellationToken.None);

        Assert.Equal("Duel", dto.Mode);
        Assert.Equal("Waiting", dto.Status);
        Assert.Equal(friend.Id, dto.InvitedUserId);
        Assert.Single(dto.Participants);
    }

    [Fact]
    public async Task Handle_NonFriend_ThrowsForbidden()
    {
        var (db, me, _, stranger) = await SeedAsync();
        var handler = new CreateDuelBattleCommandHandler(db, new FakeCurrentUserService { UserId = me.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new CreateDuelBattleCommand(stranger.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_Self_ThrowsConflict()
    {
        var (db, me, _, _) = await SeedAsync();
        var handler = new CreateDuelBattleCommandHandler(db, new FakeCurrentUserService { UserId = me.Id }, new FakeRealtimeNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new CreateDuelBattleCommand(me.Id), CancellationToken.None));
    }
}
