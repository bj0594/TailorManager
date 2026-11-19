using TailorManager.Core;

namespace TailorManager.Tests;

public class OrderServiceTests
{
    // The services with fake repositories, a customer and an employee ready to use (TestData/FakeServices.cs).
    private readonly FakeServices _app = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = _app.Orders;
    }

    [Fact]
    public void AddOrder_ValidInput_ReturnsTheOrder()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, Examples.WorkDescription, Examples.AgreedPrice, null, _app.Employee);

        // Assert
        Assert.Equal(Examples.WorkDescription, order.WorkDescription);
    }

    [Fact]
    public void AddOrder_WithEmployee_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, Examples.WorkDescription, Examples.AgreedPrice, null, _app.Employee);

        // Assert
        Assert.Equal(Examples.EmployeeDisplayName, order.History.First().ActorDisplayName);
    }

    [Fact]
    public void GetOrder_ExistingId_ReturnsThatOrder()
    {
        // Arrange
        _app.AddOrder(workDescription: Examples.OtherWorkDescription);
        var order = _app.AddOrder();

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Equal(order.Id, found.Id);
    }

    [Fact]
    public void StartOrder_ExistingOrder_SetsInProgress()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        _service.StartOrder(order.Id);

        // Assert
        Assert.Equal(OrderStatus.InProgress, _service.GetOrder(order.Id).Status);
    }
}
