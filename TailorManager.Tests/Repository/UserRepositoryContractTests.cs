using TailorManager.Core;
using TailorManager.Core.Repository;

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
    public void GetByUsername_AfterAdd_KeepsTheNamesAndTheRole()
    {
        // Arrange
        var repository = CreateRepository();
        var username = $"user-{Guid.NewGuid()}";
        repository.Add(new User { Id = Guid.NewGuid(), Username = username, PasswordHash = "hash", FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk });

        // Act
        var user = repository.GetByUsername(username);

        // Assert
        Assert.Equal("Kari", user?.FirstName);
        Assert.Equal("Nordmann", user?.LastName);
        Assert.Equal(Roles.FrontDesk, user?.Role);
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
