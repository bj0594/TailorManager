using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// The same tests for every implementation of IOrderRepository.
public abstract class OrderRepositoryContractTests
{
    private const string WorkDescription = "Shorten the trousers by 3 cm";

    protected abstract IOrderRepository CreateRepository();

    [Fact]
    public void Add_ThenGetById_ReturnsTheSameObject()
    {
        // Arrange
        var repository = CreateRepository();
        var order = NewOrder();

        // Act
        repository.Add(order);

        // Assert
        var found = repository.GetById(order.Id);
        Assert.NotNull(found);
        Assert.Equal(WorkDescription, found.WorkDescription);
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
        var first = NewOrder();
        var second = NewOrder("Replace zipper on jacket");
        repository.Add(first);
        repository.Add(second);

        // Act
        var all = repository.GetAll();

        // Assert
        Assert.Contains(all, o => o.Id == first.Id);
        Assert.Contains(all, o => o.Id == second.Id);
    }

    [Fact]
    public void Update_ChangedOrder_IsStored()
    {
        // Arrange
        var repository = CreateRepository();
        var order = NewOrder();
        repository.Add(order);

        // Act
        order.Status = OrderStatus.InProgress;
        repository.Update(order);

        // Assert
        Assert.Equal(OrderStatus.InProgress, repository.GetById(order.Id)!.Status);
    }

    private static Order NewOrder(string workDescription = WorkDescription) => new()
    {
        Id = Guid.NewGuid(),
        CustomerId = Guid.NewGuid(),
        Type = OrderType.NewGarment,
        WorkDescription = workDescription,
        InitialAgreedPrice = 450m
    };
}

public class FakeOrderRepositoryTests : OrderRepositoryContractTests
{
    protected override IOrderRepository CreateRepository() => new FakeOrderRepository();
}
