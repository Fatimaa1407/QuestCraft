using Microsoft.EntityFrameworkCore;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Battles;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Battles;

public class SubmitBattleSolutionCommandHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User A, User B, Battle Battle)> SeedInProgressAsync(string? dbName = null)
    {
        var db = dbName is null ? InMemoryDbContextFactory.Create() : InMemoryDbContextFactory.CreateSharedInstance(dbName);
        var role = new Role { Name = "Student" };
        db.Roles.Add(role);
        await db.SaveChangesAsync();

        var userA = await BattleTestSupport.CreateUserAsync(db, "userA", role.Id);
        var userB = await BattleTestSupport.CreateUserAsync(db, "userB", role.Id);
        var challenge = await BattleTestSupport.CreateBattleChallengeAsync(db, xpReward: 40, coinReward: 15);

        var battle = new Battle
        {
            Mode = BattleMode.Duel, Status = BattleStatus.InProgress, MaxPlayers = 2, ChallengeId = challenge.Id,
            HostUserId = userA.Id, StartedAt = DateTime.UtcNow, ParticipantCount = 2,
        };
        battle.Participants.Add(new BattleParticipant { UserId = userA.Id, TotalTestCases = 1 });
        battle.Participants.Add(new BattleParticipant { UserId = userB.Id, TotalTestCases = 1 });
        db.Battles.Add(battle);
        await db.SaveChangesAsync();

        return (db, userA, userB, battle);
    }

    private static SubmitBattleSolutionCommandHandler MakeHandler(ApplicationDbContext db, int userId, bool allPass, FakeRealtimeNotifier? notifier = null) =>
        new(db, new FakeCurrentUserService { UserId = userId }, new FakeCodeExecutionEngine { AllPass = allPass },
            new FakeBattleHubNotifier(), new AchievementEvaluator(db), notifier ?? new FakeRealtimeNotifier());

    [Fact]
    public async Task Handle_NonParticipant_ThrowsForbidden()
    {
        var (db, _, _, battle) = await SeedInProgressAsync();
        var handler = MakeHandler(db, userId: 999_999, allPass: true);

        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_BattleNotInProgress_ThrowsConflict()
    {
        var (db, userA, _, battle) = await SeedInProgressAsync();
        battle.Status = BattleStatus.Finished;
        await db.SaveChangesAsync();
        var handler = MakeHandler(db, userA.Id, allPass: true);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AlreadyFinished_ThrowsConflict()
    {
        var (db, userA, _, battle) = await SeedInProgressAsync();
        var handler = MakeHandler(db, userA.Id, allPass: true);
        await handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code"), CancellationToken.None);

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code v2"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_FirstFullSolve_WinsAndGrantsReward()
    {
        var (db, userA, _, battle) = await SeedInProgressAsync();
        var notifier = new FakeRealtimeNotifier();
        var handler = MakeHandler(db, userA.Id, allPass: true, notifier);

        var result = await handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code"), CancellationToken.None);

        Assert.True(result.AllPassed);
        Assert.Equal("Finished", result.Battle.Status);
        var winnerEntry = result.Battle.Participants.Single(p => p.UserId == userA.Id);
        Assert.Equal(1, winnerEntry.Rank);

        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == userA.Id);
        Assert.Equal(40, profile.Xp);
        Assert.Equal(15, profile.Coins);
        Assert.True(await db.Notifications.AnyAsync(n => n.UserId == userA.Id));
        Assert.Contains(userA.Id, notifier.NotifiedUserIds);
    }

    [Fact]
    public async Task Handle_PartialSolve_NoRewardAndBattleStillInProgress()
    {
        var (db, userA, _, battle) = await SeedInProgressAsync();
        var handler = MakeHandler(db, userA.Id, allPass: false);

        var result = await handler.Handle(new SubmitBattleSolutionCommand(battle.Id, "code"), CancellationToken.None);

        Assert.False(result.AllPassed);
        Assert.Equal("InProgress", result.Battle.Status);
        var profile = await db.UserProfiles.FirstAsync(p => p.UserId == userA.Id);
        Assert.Equal(0, profile.Xp);
    }

    [Fact]
    public async Task Handle_SimultaneousFullSolves_SecondIsRankedNotCorrupted()
    {
        var dbName = Guid.NewGuid().ToString();
        var (seedDb, userA, userB, battle) = await SeedInProgressAsync(dbName);
        await seedDb.DisposeAsync();

        // Both contexts force-track the InProgress battle before either submits, mirroring two
        // genuinely concurrent submit requests that both read the battle while it's still open.
        await using var dbA = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        await using var dbB = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        await dbA.Battles.Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Statistics)
            .Include(b => b.Challenge).ThenInclude(c => c.TestCases)
            .Include(b => b.Challenge).ThenInclude(c => c.HiddenTestCases)
            .FirstAsync(b => b.Id == battle.Id);
        await dbB.Battles.Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Profile)
            .Include(b => b.Participants).ThenInclude(p => p.User).ThenInclude(u => u.Statistics)
            .Include(b => b.Challenge).ThenInclude(c => c.TestCases)
            .Include(b => b.Challenge).ThenInclude(c => c.HiddenTestCases)
            .FirstAsync(b => b.Id == battle.Id);

        var handlerA = MakeHandler(dbA, userA.Id, allPass: true);
        var handlerB = MakeHandler(dbB, userB.Id, allPass: true);

        var resultA = await handlerA.Handle(new SubmitBattleSolutionCommand(battle.Id, "codeA"), CancellationToken.None);
        // B's context still holds the pre-A-commit snapshot, so its own concurrency-conflict recovery
        // path (not a hard failure) must kick in and rank it 2nd against the now-real finisher count.
        var resultB = await handlerB.Handle(new SubmitBattleSolutionCommand(battle.Id, "codeB"), CancellationToken.None);

        Assert.Equal(1, resultA.Battle.Participants.Single(p => p.UserId == userA.Id).Rank);

        await using var verifyDb = InMemoryDbContextFactory.CreateSharedInstance(dbName);
        var finalParticipants = await verifyDb.BattleParticipants.Where(p => p.BattleId == battle.Id).ToListAsync();
        Assert.Equal(1, finalParticipants.Single(p => p.UserId == userA.Id).Rank);
        Assert.Equal(2, finalParticipants.Single(p => p.UserId == userB.Id).Rank);
        // Only the true winner (userA) was ever rewarded — B's reconciled 2nd-place finish grants nothing.
        var profileA = await verifyDb.UserProfiles.FirstAsync(p => p.UserId == userA.Id);
        var profileB = await verifyDb.UserProfiles.FirstAsync(p => p.UserId == userB.Id);
        Assert.Equal(40, profileA.Xp);
        Assert.Equal(0, profileB.Xp);
    }

    [Fact]
    public async Task Handle_NearIdenticalSubmissions_FlagsSimilarityAuditLog()
    {
        var (db, userA, userB, battle) = await SeedInProgressAsync();
        const string sharedCode = @"
using System;
class Solution {
    static void Main() {
        int total = 0;
        for (int i = 0; i < 10; i++) { total += i; }
        Console.WriteLine(total);
    }
}";
        // B submits a losing/partial attempt first (still records SubmittedCode even though it fails),
        // then A submits byte-identical code and wins outright — the similarity check compares against
        // anyone who submitted anything in the battle, not just other full finishers. Verbatim
        // copy-paste (not a cleverly-renamed near-miss) is the realistic case this heuristic targets;
        // see CodeSimilarity's doc comment for why renaming defeats it.
        var handlerB = MakeHandler(db, userB.Id, allPass: false);
        await handlerB.Handle(new SubmitBattleSolutionCommand(battle.Id, sharedCode), CancellationToken.None);

        var handlerA = MakeHandler(db, userA.Id, allPass: true);
        await handlerA.Handle(new SubmitBattleSolutionCommand(battle.Id, sharedCode), CancellationToken.None);

        Assert.True(await db.AuditLogs.AnyAsync(a => a.Action == "BattleSimilarityFlagged" && a.EntityId == battle.Id));
    }
}
