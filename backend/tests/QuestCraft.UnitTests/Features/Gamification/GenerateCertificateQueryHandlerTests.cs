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
    private static async Task<(ApplicationDbContext Db, User User)> SeedAsync(int level)
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
            Profile = new UserProfile { Level = level, Xp = 100 },
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        return (db, user);
    }

    [Fact]
    public async Task Handle_BelowRequiredLevel_ThrowsForbidden()
    {
        var (db, user) = await SeedAsync(level: 5);
        var handler = new GenerateCertificateQueryHandler(db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator());

        await Assert.ThrowsAsync<ForbiddenException>(
            () => handler.Handle(new GenerateCertificateQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_AtRequiredLevel_ReturnsPdfBytes()
    {
        var (db, user) = await SeedAsync(level: 10);
        var handler = new GenerateCertificateQueryHandler(db, new FakeCurrentUserService { UserId = user.Id }, new FakeCertificatePdfGenerator());

        var result = await handler.Handle(new GenerateCertificateQuery(), CancellationToken.None);

        Assert.NotEmpty(result);
    }
}
