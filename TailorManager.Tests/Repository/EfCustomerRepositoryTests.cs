using TailorManager.Core.Repository;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

// The customer contract tests against PostgreSQL.
public class EfCustomerRepositoryTests : CustomerRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfCustomerRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override ICustomerRepository CreateRepository() => new EfCustomerRepository(_db.CreateContext());
}
