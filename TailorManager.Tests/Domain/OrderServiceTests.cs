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
        _service.StartOrder(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.InProgress, _service.GetOrder(order.Id).Status);
    }

    [Fact]
    public void StartOrder_ExistingOrder_AddsStatusChangedEntry()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        var result = _service.StartOrder(order.Id, _app.Employee);

        // Assert
        var entry = Assert.Single(result.History, h => h.Type == ActivityType.StatusChanged);
        Assert.Equal(Examples.EmployeeDisplayName, entry.ActorDisplayName);
    }

    [Fact]
    public void StartOrder_NotSubmitted_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var order = _app.AddOrder();
        _service.StartOrder(order.Id, _app.Employee);

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.StartOrder(order.Id, _app.Employee));
    }

    [Fact]
    public void AddWorkNote_OrderInProgress_AddsWorkNote()
    {
        // Arrange
        var order = _app.AddOrder();
        _service.StartOrder(order.Id, _app.Employee);

        // Act
        _service.AddWorkNote(order.Id, Examples.WorkNote, _app.Employee);

        // Assert
        var entry = Assert.Single(_service.GetOrder(order.Id).History, h => h.Type == ActivityType.WorkNoteAdded);
        Assert.Equal(Examples.WorkNote, entry.Note);
    }

    [Fact]
    public void MarkReadyForPickup_OrderInProgress_SetsReadyForPickup()
    {
        // Arrange
        var order = _app.AddOrder();
        _service.StartOrder(order.Id, _app.Employee);

        // Act
        var result = _service.MarkReadyForPickup(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.ReadyForPickup, result.Status);
        Assert.Equal(2, result.History.Count(h => h.Type == ActivityType.StatusChanged));
        Assert.Single(result.History, h => h.Type == ActivityType.NotificationRequested);
    }

    [Fact]
    public void AddPayment_ExistingOrder_AddsPayment()
    {
        // Arrange
        var order = _app.AddOrder();
        const decimal paymentAmount = 500m;

        // Act
        _service.AddPayment(order.Id, paymentAmount);

        // Assert
        var payment = Assert.Single(_service.GetOrder(order.Id).Payments);
        Assert.Equal(paymentAmount, payment.Amount);
    }

    [Fact]
    public void AddOrder_FreeOrder_HasZeroOutstandingBalance()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(
            customer.Id,
            OrderType.NewGarment,
            Examples.WorkDescription,
            0m,
            null,
            _app.Employee);

        // Assert
        Assert.Equal(0m, order.OutstandingBalance);
    }

    [Fact]
    public void AddOrder_WithAgreedPrice_HasFullOutstandingBalance()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(
            customer.Id,
            OrderType.NewGarment,
            Examples.WorkDescription,
            Examples.AgreedPrice,
            null,
            _app.Employee);

        // Assert
        Assert.Equal(Examples.AgreedPrice, order.OutstandingBalance);
    }

    [Fact]
    public void DeliverOrder_NoOutstandingBalance_SetsDelivered()
    {
        // Arrange
        var order = _app.AddOrder();

        _service.AddPayment(order.Id, order.OutstandingBalance);
        _service.MarkReadyForPickup(order.Id, _app.Employee);

        // Act
        _service.DeliverOrder(order.Id);

        // Assert
        Assert.True(_service.GetOrder(order.Id).IsDelivered);
    }
}
