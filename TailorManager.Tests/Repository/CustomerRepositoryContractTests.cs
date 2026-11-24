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
        var customer = TestData.NewCustomer();

        // Act
        repository.Add(customer);

        // Assert
        var found = repository.GetById(customer.Id);
        Assert.NotNull(found);
        Assert.Equal(Examples.CustomerName, found.Name);
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
    public void GetAll_AfterTwoAdded_ContainsBoth()
    {
        // Arrange
        var repository = CreateRepository();
        var first = TestData.NewCustomer();
        var second = TestData.NewCustomer(Examples.OtherCustomerName);
        repository.Add(first);
        repository.Add(second);

        // Act
        var all = repository.GetAll();

        // Assert
        Assert.Contains(all, c => c.Id == first.Id);
        Assert.Contains(all, c => c.Id == second.Id);
    }

    [Fact]
    public void Find_ByName_ReturnsOnlyMatching()
    {
        // Arrange
        var repository = CreateRepository();
        var searchTerm = Guid.NewGuid().ToString();
        var match = TestData.NewCustomer(searchTerm);
        repository.Add(match);
        repository.Add(TestData.NewCustomer(Examples.OtherCustomerName));

        // Act
        var found = repository.Find(searchTerm);

        // Assert
        Assert.Equal(match.Id, Assert.Single(found).Id);
    }
}

public class FakeCustomerRepositoryTests : CustomerRepositoryContractTests
{
    protected override ICustomerRepository CreateRepository() => new FakeCustomerRepository();
}
