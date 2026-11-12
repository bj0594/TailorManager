using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;
using TailorManager.Core.Repository;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

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

public class EfCustomerRepositoryTests : CustomerRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfCustomerRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override ICustomerRepository CreateRepository() => new EfCustomerRepository(_db.CreateContext());
}

public class EfOrderRepositoryTests : OrderRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfOrderRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override IOrderRepository CreateRepository() => new EfOrderRepository(_db.CreateContext());
}
