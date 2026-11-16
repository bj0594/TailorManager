using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// The same tests for every implementation of IOrderRepository.
public abstract class OrderRepositoryContractTests
{
    protected abstract IOrderRepository CreateRepository();

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        // Arrange
        var repository = CreateRepository();
        var order = new Order { Id = Guid.NewGuid(), WorkDescription = "Shorten trousers by 3cm" };

        // Act
        repository.Add(order);

        // Assert
        var found = repository.GetById(order.Id);
        Assert.NotNull(found);
        Assert.Equal("Shorten trousers by 3cm", found.WorkDescription);
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
    public void GetAllOrders_AfterTwoAdded_ContainsBoth()
    {
        // Arrange
        var repository = CreateRepository();
        var first = new Order { Id = Guid.NewGuid(), WorkDescription = "Shorten trousers by 3cm" };
        var second = new Order { Id = Guid.NewGuid(), WorkDescription = "Replace zipper on jacket" };
        repository.Add(first);
        repository.Add(second);

        // Act
        var all = repository.GetAll();

        // Assert
        Assert.Contains(all, o => o.Id == first.Id);
        Assert.Contains(all, o => o.Id == second.Id);
    }
}

public class FakeOrderRepositoryTests : OrderRepositoryContractTests
{
    protected override IOrderRepository CreateRepository() => new FakeOrderRepository();
}
