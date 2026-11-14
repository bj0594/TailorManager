using TailorManager.Core;
using TailorManager.Core.Repository;
using TailorManager.Infrastructure;

namespace TailorManager.Tests;

// The same tests for every implementation of IUserRepository.
public abstract class UserRepositoryContractTests
{
    protected abstract IUserRepository CreateRepository();

    [Fact]
    public void GetByUsername_AfterAdd_ReturnsTheUser()
    {
        // Arrange
        var repository = CreateRepository();
        var username = $"user-{Guid.NewGuid()}";
        repository.Add(new User { Id = Guid.NewGuid(), Username = username, PasswordHash = "hash" });

        // Act
        var user = repository.GetByUsername(username);

        // Assert
        Assert.Equal("hash", user?.PasswordHash);
    }

    [Fact]
    public void GetByUsername_Unknown_ReturnsNull()
    {
        // Act
        var user = CreateRepository().GetByUsername($"nobody-{Guid.NewGuid()}");

        // Assert
        Assert.Null(user);
    }
}

public class FakeUserRepositoryTests : UserRepositoryContractTests
{
    protected override IUserRepository CreateRepository() => new FakeUserRepository();
}

public class EfUserRepositoryTests : UserRepositoryContractTests, IClassFixture<EfDatabaseFixture>
{
    private readonly EfDatabaseFixture _db;

    public EfUserRepositoryTests(EfDatabaseFixture db) => _db = db;

    protected override IUserRepository CreateRepository() => new EfUserRepository(_db.CreateContext());
}
