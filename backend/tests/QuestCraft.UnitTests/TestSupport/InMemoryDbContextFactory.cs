using Microsoft.EntityFrameworkCore;
using QuestCraft.Infrastructure.Persistence;

namespace QuestCraft.UnitTests.TestSupport;

public static class InMemoryDbContextFactory
{
    // A fresh, isolated database per call (unique name) so tests never see each other's data.
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    // Two separate context instances sharing one named database — for tests that need to simulate
    // two concurrent requests racing against the same rows (each real HTTP request gets its own
    // scoped DbContext in production, so a single shared context wouldn't model the race at all).
    public static ApplicationDbContext CreateSharedInstance(string name)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(name)
            .Options;
        return new ApplicationDbContext(options);
    }
}
