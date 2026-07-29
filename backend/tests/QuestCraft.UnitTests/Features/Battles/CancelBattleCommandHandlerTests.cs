using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class CancelBattleCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User Host, User Other, Battle Battle)> SeedAsync(BattleStatus status = BattleStatus.Waiting)
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var other = await BattleTestSupport.CreateUserAsync(db, "other", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Room, Status = status, MaxPlayers = 4, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        return (db, host, other, battle);
    }

    [Fact]
    public async Task Handle_HostCancelsWaitingBattle_Cancels()
    {
        var (db, host, _, battle) = await SeedAsync();
        var handler = new CancelBattleCommandHandler(db, new FakeCurrentUserService { UserId = host.Id }, new FakeBattleHubNotifier());

        await handler.Handle(new CancelBattleCommand(battle.Id), CancellationToken.None);

        var updated = await db.Battles.FindAsync(battle.Id);
        Assert.Equal(BattleStatus.Cancelled, updated!.Status);
    }

    [Fact]
    public async Task Handle_NotHost_ThrowsForbidden()
    {
        var (db, _, other, battle) = await SeedAsync();
        var handler = new CancelBattleCommandHandler(db, new FakeCurrentUserService { UserId = other.Id }, new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new CancelBattleCommand(battle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyInProgress_ThrowsConflict()
    {
        var (db, host, _, battle) = await SeedAsync(BattleStatus.InProgress);
        var handler = new CancelBattleCommandHandler(db, new FakeCurrentUserService { UserId = host.Id }, new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new CancelBattleCommand(battle.Id), CancellationToken.None));
    }
}
