using TailorManager.Core.Repository;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

// The user contract tests against PostgreSQL.
public class EfUserRepositoryTests : UserRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfUserRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override IUserRepository CreateRepository() => new EfUserRepository(_db.CreateContext());
}
