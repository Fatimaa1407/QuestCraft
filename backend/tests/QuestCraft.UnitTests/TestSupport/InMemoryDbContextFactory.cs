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
}
