using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public abstract class OrderRepositoryContractTests
{
    protected abstract IOrderRepository CreateRepository();

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        var repository = CreateRepository();
        var order = new Order { Id = Guid.NewGuid(), WorkDescription = "Shorten trousers by 3cm" };

        repository.Add(order);

        var found = repository.GetById(order.Id);
        Assert.NotNull(found);
        Assert.Equal("Shorten trousers by 3cm", found.WorkDescription);
    }

    [Fact]
    public void GetById_UnknownId_ReturnsNull()
    {
        var repository = CreateRepository();

        Assert.Null(repository.GetById(Guid.NewGuid()));
    }
}

public class FakeOrderRepositoryTests : OrderRepositoryContractTests
{
    protected override IOrderRepository CreateRepository() => new FakeOrderRepository();
}
