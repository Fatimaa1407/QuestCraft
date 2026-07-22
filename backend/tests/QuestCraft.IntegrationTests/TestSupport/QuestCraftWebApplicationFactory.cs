using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuestCraft.Infrastructure.Persistence;

namespace QuestCraft.IntegrationTests.TestSupport;

// Runs the real API pipeline (controllers, MediatR, validators, JWT auth) against an isolated
// Sqlite in-memory database instead of the real SQL Server instance. Sqlite (not the EF Core
// InMemory provider) because TransactionBehavior wraps every MediatR command in
// Database.BeginTransactionAsync(), which only a real relational provider supports.
//
// Program.cs reads `builder.Configuration.GetSection("Jwt")` directly, BEFORE `builder.Build()` —
// so WebApplicationFactory's usual ConfigureWebHost/ConfigureAppConfiguration hooks (which only
// take effect once .Build() runs, via a deferred host-builder shim) are too late to supply it.
// Real process environment variables, set here in the constructor before the host is ever
// created, are the one thing `WebApplication.CreateBuilder(args)` picks up early enough.
public class QuestCraftWebApplicationFactory : WebApplicationFactory<Program>
{
    // Must stay open for the process lifetime — an in-memory Sqlite database is destroyed the
    // moment its one and only connection closes.
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public QuestCraftWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        Environment.SetEnvironmentVariable("Jwt__Secret", "integration-test-signing-key-needs-to-be-at-least-32-chars");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "QuestCraft.API.Tests");
        Environment.SetEnvironmentVariable("Jwt__Audience", "QuestCraft.Client.Tests");
        Environment.SetEnvironmentVariable("Jwt__AccessTokenExpirationMinutes", "15");
        Environment.SetEnvironmentVariable("Jwt__RefreshTokenExpirationDays", "7");
        Environment.SetEnvironmentVariable("Cors__AllowedOrigins__0", "http://localhost");

        // Build the schema BEFORE the host (and its hosted services, e.g. WeeklyRecapBackgroundService)
        // ever starts — starting the host first and calling EnsureCreated() afterwards races the
        // background service's own DbContext for the same shared Sqlite connection, and Sqlite
        // doesn't allow two overlapping transactions on one connection.
        _connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(_connection).Options;
        using var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            // EF Core 8+ also accumulates per-context options configuration in this collection
            // separately from the DbContextOptions<T> singleton above — without removing it too,
            // AddInfrastructure's UseSqlServer(...) call stays merged in alongside UseSqlite(...)
            // below, and EF throws "Multiple relational database provider configurations found."
            services.RemoveAll(typeof(IDbContextOptionsConfiguration<ApplicationDbContext>));

            // AddInfrastructure already added SqlServer's provider services to this same
            // IServiceCollection via UseSqlServer(...) — removing the DbContextOptions<T>
            // registration alone doesn't remove those, so EF sees two registered providers and
            // throws. Giving Sqlite its own dedicated internal service provider sidesteps that
            // instead of trying to surgically unpick SqlServer's registrations.
            var sqliteInternalServices = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
                options.UseInternalServiceProvider(sqliteInternalServices);
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
