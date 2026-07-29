using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class AutoResolveStaleBattlesCommandHandlerTests
{
    private static AutoResolveStaleBattlesCommandHandler MakeHandler(ApplicationDbContext db) =>
        new(db, new FakeBattleHubNotifier(), new AchievementEvaluator(db), new FakeRealtimeNotifier());

    [Fact]
    public async Task Handle_StaleWaitingBattle_GetsCancelled()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Room, Status = BattleStatus.Waiting, MaxPlayers = 2, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();
        battle.CreatedAt = DateTime.UtcNow - AutoResolveStaleBattlesCommandHandler.WaitingTimeout - TimeSpan.FromMinutes(1);
        await db.SaveChangesAsync();

        var resolvedCount = await MakeHandler(db).Handle(new AutoResolveStaleBattlesCommand(), CancellationToken.None);

        Assert.Equal(1, resolvedCount);
        var updated = await db.Battles.FindAsync(battle.Id);
        Assert.Equal(BattleStatus.Cancelled, updated!.Status);
    }

    [Fact]
    public async Task Handle_FreshWaitingBattle_LeftAlone()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var host = await BattleTestSupport.CreateUserAsync(db, "host", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db);

        var battle = new Battle { Mode = BattleMode.Room, Status = BattleStatus.Waiting, MaxPlayers = 2, ChallengeId = challenge.Id, HostUserId = host.Id, ParticipantCount = 1 };
        battle.Participants.Add(new BattleParticipant { UserId = host.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var resolvedCount = await MakeHandler(db).Handle(new AutoResolveStaleBattlesCommand(), CancellationToken.None);

        Assert.Equal(0, resolvedCount);
        var updated = await db.Battles.FindAsync(battle.Id);
        Assert.Equal(BattleStatus.Waiting, updated!.Status);
    }

    [Fact]
    public async Task Handle_StaleInProgressBattle_ResolvesByPartialProgressWithNoReward()
    {
        var db = InMemoryDbContextFactory.Create();
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        var userA = await BattleTestSupport.CreateUserAsync(db, "userA", role.Id);
        var userB = await BattleTestSupport.CreateUserAsync(db, "userB", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db, xpReward: 40, coinReward: 15);

        var battle = new Battle
        {
            Mode = BattleMode.Duel, Status = BattleStatus.InProgress, MaxPlayers = 2, ChallengeId = challenge.Id,
            HostUserId = userA.Id, StartedAt = DateTime.UtcNow - AutoResolveStaleBattlesCommandHandler.InProgressTimeout - TimeSpan.FromMinutes(1),
            ParticipantCount = 2,
        };
        battle.Participants.Add(new BattleParticipant { UserId = userA.Id, TotalTestCases = 1, PassedTestCases = 1 });
        battle.Participants.Add(new BattleParticipant { UserId = userB.Id, TotalTestCases = 1, PassedTestCases = 0 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        var resolvedCount = await MakeHandler(db).Handle(new AutoResolveStaleBattlesCommand(), CancellationToken.None);

        Assert.Equal(1, resolvedCount);
        var updated = await db.Battles.FindAsync(battle.Id);
        Assert.Equal(BattleStatus.Finished, updated!.Status);

        var participantA = await db.BattleParticipants.FirstAsync(p => p.BattleId == battle.Id && p.UserId == userA.Id);
        Assert.Equal(1, participantA.Rank);

        // Nobody actually finished (fully passed) before the timeout, so nobody gets rewarded even
        // though userA has the higher partial score and is ranked 1st for the scoreboard.
        var profileA = await db.UserProfiles.FirstAsync(p => p.UserId == userA.Id);
        Assert.Equal(0, profileA.Xp);
    }
}
