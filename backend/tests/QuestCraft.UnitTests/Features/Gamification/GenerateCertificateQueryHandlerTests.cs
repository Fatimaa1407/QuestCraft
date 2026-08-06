using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Domain.Enums;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class FakeCertificatePdfGenerator : ICertificatePdfGenerator
{
    public byte[] Generate(CertificateData data) => [1, 2, 3];
}

public class GenerateCertificateQueryHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User, Challenge Challenge)> SeedAsync(
        int level, DateTime? gameCompletedAt, bool solveChallenge)
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
            Profile = new UserProfile { Level = level, Xp = 100, GameCompletedAt = gameCompletedAt },
        };
        db.Users.Add(user);

        // The only published content, at level 1 — so GetMaxAvailableLevelAsync resolves to 1 and
        // "fully complete" always means exactly "this one challenge is solved", keeping every test
        // here about the certificate gate itself rather than juggling a bigger content tree.
        var challenge = new Challenge
        {
            Title = "L1", Description = "d", StarterCode = "c", CategoryId = 1, DifficultyId = 1,
            RequiredLevel = 1, IsPublished = true,
        };
        db.Challenges.Add(challenge);
        await db.SaveChangesAsync();

        if (solveChallenge)
        {
            db.ChallengeSubmissions.Add(new ChallengeSubmission
            {
                UserId = user.Id,
                ChallengeId = challenge.Id,
                Verdict = SubmissionVerdict.Accepted,
                SourceCode = "x",
                SubmittedAt = DateTime.UtcNow,
            });
            await db.SaveChangesAsync();
        }

        return (db, user, challenge);
    }

    [Fact]
    public async Task Handle_GameNotCompleted_ThrowsForbidden()
    {
        // Tied to actually finishing the game, not a hardcoded level number — reaching a high level
        // (e.g. via CalculateUnlockedLevelAsync capping at the current max) isn't enough on its own.
        var (db, user, _) = await SeedAsync(level: 12, gameCompletedAt: null, solveChallenge: false);
        var handler = new GenerateCertificateQueryHandler(
            db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator(), new ContentCompletionService(db));

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GenerateCertificateQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GameCompletedButContentActuallyIncomplete_ThrowsForbidden()
    {
        // Regression test: GameCompletedAt is a stored flag, not re-verified on every read — if it
        // (or Level) ever ends up set without the underlying content genuinely being 100% solved
        // (e.g. direct database editing, a bug elsewhere), the certificate must still refuse rather
        // than trusting the flag.
        var (db, user, _) = await SeedAsync(level: 1, gameCompletedAt: DateTime.UtcNow, solveChallenge: false);
        var handler = new GenerateCertificateQueryHandler(
            db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator(), new ContentCompletionService(db));

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GenerateCertificateQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GameCompleted_ReturnsPdfBytes()
    {
        var (db, user, _) = await SeedAsync(level: 1, gameCompletedAt: DateTime.UtcNow, solveChallenge: true);
        var handler = new GenerateCertificateQueryHandler(
            db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator(), new ContentCompletionService(db));

        var result = await handler.Handle(new GenerateCertificateQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
    }
}
