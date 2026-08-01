using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Common.Interfaces;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class FakeCertificatePdfGenerator : ICertificatePdfGenerator
{
    public byte[] Generate(CertificateData data) => [1, 2, 3];
}

public class GenerateCertificateQueryHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User)> SeedAsync(int level, DateTime? gameCompletedAt)
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
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_GameNotCompleted_ThrowsForbidden()
    {
        // Tied to actually finishing the game, not a hardcoded level number — reaching a high level
        // (e.g. via CalculateUnlockedLevelAsync capping at the current max) isn't enough on its own.
        var (db, user) = await SeedAsync(level: 12, gameCompletedAt: null);
        var handler = new GenerateCertificateQueryHandler(db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator());

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GenerateCertificateQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_GameCompleted_ReturnsPdfBytes()
    {
        var (db, user) = await SeedAsync(level: 12, gameCompletedAt: DateTime.UtcNow);
        var handler = new GenerateCertificateQueryHandler(db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator());

        var result = await handler.Handle(new GenerateCertificateQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
    }
}
