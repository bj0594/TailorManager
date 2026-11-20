using TailorManager.Core.Repository;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

// The order contract tests against PostgreSQL.
public class EfOrderRepositoryTests : OrderRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfOrderRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override IOrderRepository CreateRepository() => new EfOrderRepository(_db.CreateContext());

    protected override IOrderRepository Reopen(IOrderRepository repository) => CreateRepository();
}
