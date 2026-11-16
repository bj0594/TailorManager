using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// The same tests for every implementation of ICustomerRepository.
public abstract class CustomerRepositoryContractTests
{
    protected abstract ICustomerRepository CreateRepository();

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        // Arrange
        var repository = CreateRepository();
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Anna Hansen" };

        // Act
        repository.Add(customer);

        // Assert
        var found = repository.GetById(customer.Id);
        Assert.NotNull(found);
        Assert.Equal("Anna Hansen", found.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        // Arrange
        var repository = CreateRepository();

        // Act
        var found = repository.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(found);
    }

    [Fact]
    public void GetAllCustomers_AfterTwoAdded_ContainsBoth()
    {
        // Arrange
        var repository = CreateRepository();
        var first = new Customer { Id = Guid.NewGuid(), Name = "Anna Hansen" };
        var second = new Customer { Id = Guid.NewGuid(), Name = "Peter Jensen" };
        repository.Add(first);
        repository.Add(second);

        // Act
        var all = repository.GetAll();

        // Assert
        Assert.Contains(all, c => c.Id == first.Id);
        Assert.Contains(all, c => c.Id == second.Id);
    }
}

public class FakeCustomerRepositoryTests : CustomerRepositoryContractTests
{
    protected override ICustomerRepository CreateRepository() => new FakeCustomerRepository();
}
