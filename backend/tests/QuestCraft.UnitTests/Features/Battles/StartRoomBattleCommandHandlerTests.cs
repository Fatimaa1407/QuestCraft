using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class StartRoomBattleCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User Host, User Other, Battle Battle)> SeedAsync(int participantCount)
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var other = await BattleTestSupport.CreateUserAsync(db, "other", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Room, Status = BattleStatus.Waiting, MaxPlayers = 4, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = participantCount };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        if (participantCount > 1)
        {
            battle.Participants.Add(new BattleParticipant { UserId = other.Id, TotalTestCases = 1 });
        }
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        return (db, host, other, battle);
    }

    [Fact]
    public async Task Handle_HostWithEnoughPlayers_StartsBattle()
    {
        var (db, host, _, battle) = await SeedAsync(participantCount: 2);
        var handler = new StartRoomBattleCommandHandler(db, new FakeCurrentUserService { UserId = host.Id }, new FakeBattleHubNotifier());

        var dto = await handler.Handle(new StartRoomBattleCommand(battle.Id), CancellationToken.None);

        Assert.Equal("InProgress", dto.Status);
    }

    [Fact]
    public async Task Handle_NotHost_ThrowsForbidden()
    {
        var (db, _, other, battle) = await SeedAsync(participantCount: 2);
        var handler = new StartRoomBattleCommandHandler(db, new FakeCurrentUserService { UserId = other.Id }, new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new StartRoomBattleCommand(battle.Id), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NotEnoughPlayers_ThrowsConflict()
    {
        var (db, host, _, battle) = await SeedAsync(participantCount: 1);
        var handler = new StartRoomBattleCommandHandler(db, new FakeCurrentUserService { UserId = host.Id }, new FakeBattleHubNotifier());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new StartRoomBattleCommand(battle.Id), CancellationToken.None));
    }
}
