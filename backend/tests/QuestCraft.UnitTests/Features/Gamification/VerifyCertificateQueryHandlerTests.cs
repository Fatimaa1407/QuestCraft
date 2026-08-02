using QuestCraft.Application.Common;
using QuestCraft.Application.Common.Exceptions;
using QuestCraft.Application.Features.Gamification;
using QuestCraft.Domain.Entities;
using QuestCraft.Infrastructure.Persistence;
using QuestCraft.UnitTests.TestSupport;

namespace QuestCraft.UnitTests.Features.Gamification;

public class VerifyCertificateQueryHandlerTests
{
    private static async Task<(ApplicationDbContext Db, User User)> SeedCompletedUserAsync()
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
            Profile = new UserProfile { Level = 12, Xp = 4135, GameCompletedAt = DateTime.UtcNow },
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_ValidCertificateId_ReturnsMatchingUserData()
    {
        var (db, user) = await SeedCompletedUserAsync();
        var certificateId = CertificateIdGenerator.Generate(user.Id, user.Profile!.GameCompletedAt!.Value);
        var handler = new VerifyCertificateQueryHandler(db, new ContentCompletionService(db));

        var result = await handler.Handle(new VerifyCertificateQuery(certificateId), CancellationToken.None);

        Assert.Equal("Test User", result.FullName);
        Assert.Equal(certificateId, result.CertificateId);
    }

    [Fact]
    public async Task Handle_UnknownCertificateId_ThrowsNotFound()
    {
        var (db, _) = await SeedCompletedUserAsync();
        var handler = new VerifyCertificateQueryHandler(db, new ContentCompletionService(db));

        await Assert.ThrowsAsync<NotFoundException>(
            () => handler.Handle(new VerifyCertificateQuery("QC-00000000"), CancellationToken.None));
    }
}
