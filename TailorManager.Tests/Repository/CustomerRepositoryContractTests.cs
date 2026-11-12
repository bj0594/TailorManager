using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public abstract class CustomerRepositoryContractTests
{
    protected abstract ICustomerRepository CreateRepository();

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        var repository = CreateRepository();
        var customer = new Customer { Id = Guid.NewGuid(), Name = "Anna Hansen" };

        repository.Add(customer);

        var found = repository.GetById(customer.Id);
        Assert.NotNull(found);
        Assert.Equal("Anna Hansen", found.Name);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var repository = CreateRepository();

        Assert.Null(repository.GetById(Guid.NewGuid()));
    }

    [Fact]
    public void GetAllCustomers_AfterTwoAdded_ContainsBoth()
    {
        var repository = CreateRepository();
        var first = new Customer { Id = Guid.NewGuid(), Name = "Anna Hansen" };
        var second = new Customer { Id = Guid.NewGuid(), Name = "Peter Jensen" }
        ;
        repository.Add(first);
        repository.Add(second);

        var all = repository.GetAll();

        Assert.Contains(all, i => i.Id == first.Id);
        Assert.Contains(all, i => i.Id == second.Id);
    }
}

public class FakeCustomerRepositoryTests: CustomerRepositoryContractTests
{
    protected override ICustomerRepository CreateRepository () => new FakeCustomerRepository();
}
