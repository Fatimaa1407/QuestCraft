using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class JoinBattleCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User Host, User Other, Battle Battle)> SeedRoomAsync(int maxPlayers = 2)
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var other = await BattleTestSupport.CreateUserAsync(db, "other", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Room, Status = BattleStatus.Waiting, MaxPlayers = maxPlayers, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        return (db, host, other, battle);
    }

    [Fact]
    public async Task Handle_OpenRoomSlot_AddsParticipant()
    {
        var (db, _, other, battle) = await SeedRoomAsync();
        var handler = new JoinBattleCommandHandler(db, new FakeCurrentUserService { UserId = other.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());

        var dto = await handler.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None);

        Assert.Equal(2, dto.Participants.Count);
        Assert.Contains(dto.Participants, p => p.UserId == other.Id);
    }

    [Fact]
    public async Task Handle_RoomFull_ThrowsConflict()
    {
        var (db, _, other, battle) = await SeedRoomAsync(maxPlayers: 1);
        var handler = new JoinBattleCommandHandler(db, new FakeCurrentUserService { UserId = other.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyJoined_ThrowsConflict()
    {
        var (db, host, _, battle) = await SeedRoomAsync();
        var handler = new JoinBattleCommandHandler(db, new FakeCurrentUserService { UserId = host.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DuelWrongInvitedUser_ThrowsForbidden()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var invited = await BattleTestSupport.CreateUserAsync(db, "invited", role.Id);
        var stranger = await BattleTestSupport.CreateUserAsync(db, "stranger", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Duel, Status = BattleStatus.Waiting, MaxPlayers = 2, ChallengeId = challenge.Id, HostUserId = host.Id, InvitedUserId = invited.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var handler = new JoinBattleCommandHandler(db, new FakeCurrentUserService { UserId = stranger.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_TwoUsersRaceForLastSlot_SecondGetsConflict()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var seedDb = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        var role = new Role { Name = "Student" };
        seedDb.Roles.Add(role);
        await seedDb.SaveChangesAsync();

        var host = await BattleTestSupport.CreateUserAsync(seedDb, "host", role.Id);
        var userA = await BattleTestSupport.CreateUserAsync(seedDb, "userA", role.Id);
        var userB = await BattleTestSupport.CreateUserAsync(seedDb, "userB", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(seedDb);

        var battle = new Battle { Mode = BattleMode.Room, Status = BattleStatus.Waiting, MaxPlayers = 2, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        seedDb.Battles.Add(battle);
        await seedDb.SaveChangesAsync();

        // Two independent DbContext instances (mirroring two separate concurrent HTTP requests, each
        // with its own scoped context) both read the room while it still has exactly 1 open slot. B's
        // context is force-tracked with that stale snapshot up front — a real second request would
        // have loaded it at roughly the same moment as A, before A's commit, so its handler's own
        // query later must not silently pick up A's already-committed change (EF Core's identity map
        // won't overwrite an already-tracked entity from a fresh query, which is exactly the point:
        // this reproduces what a genuinely concurrent read would see).
        await using var dbA = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        await using var dbB = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        await dbB.Battles.Include(b => b.Participants).FirstAsync(b => b.Id == battle.Id);

        var handlerA = new JoinBattleCommandHandler(dbA, new FakeCurrentUserService { UserId = userA.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());
        var handlerB = new JoinBattleCommandHandler(dbB, new FakeCurrentUserService { UserId = userB.Id }, new FakeRealtimeNotifier(), new FakeBattleHubNotifier());

        await handlerA.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None);

        // B's context still holds the stale (pre-A-commit) RowVersion it read earlier, so its write
        // must lose the race — proving a 2-slot room can never end up with 3 participants.
        await Assert.ThrowsAsync<ConflictException>(() => handlerB.Handle(new JoinBattleCommand(battle.Id), CancellationToken.None));

        await using var verifyDb = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        var finalCount = verifyDb.BattleParticipants.Count(p => p.BattleId == battle.Id);
        Assert.Equal(2, finalCount);
    }
}
