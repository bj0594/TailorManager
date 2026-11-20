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

    [Fact]
    public void StartOrder_NotSubmitted_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var order = _app.AddOrder();
        _service.StartOrder(order.Id);

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.StartOrder(order.Id));
    }

    [Fact]
    public void AddWorkNote_OrderInProgress_AddsWorkNote()
    {
        // Arrange
        var order = _app.AddOrder();
        _service.StartOrder(order.Id);

        // Act
        _service.AddWorkNote(order.Id, Examples.WorkNote, _app.Employee);

        // Assert
        var entry = Assert.Single(_service.GetOrder(order.Id).History, h => h.Type == ActivityType.WorkNoteAdded);
        Assert.Equal(Examples.WorkNote, entry.Note);
    }

    [Fact]
    public void MarkOrder_ExistingOrder_SetsReadyForPickup()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        _service.ReadyForPickup(order.Id);

        // Assert
        Assert.True(_service.GetOrder(order.Id).IsReadyForPickup);
    }
}
