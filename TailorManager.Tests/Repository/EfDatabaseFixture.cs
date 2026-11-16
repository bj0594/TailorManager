using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

// Starts PostgreSQL in Docker once per test class, and runs the migrations, so the EF tests use a real database.
public class EfDatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:17").Build();

    public TailorManagerDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<TailorManagerDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options);

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    public Task DisposeAsync() => _container.DisposeAsync().AsTask();
}
