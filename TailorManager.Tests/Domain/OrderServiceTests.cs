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
        Assert.Equal(Examples.AgreedPrice, order.InitialAgreedPrice);
        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(ActivityType.OrderCreated, Assert.Single(order.History).Type);
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
        var order = _app.AddOrderInProgress();

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.StartOrder(order.Id, _app.Employee));
    }

    [Fact]
    public void AddWorkNote_OrderInProgress_AddsWorkNote()
    {
        // Arrange
        var order = _app.AddOrderInProgress();

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
        var order = _app.AddOrderInProgress();

        // Act
        var result = _service.MarkReadyForPickup(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.ReadyForPickup, result.Status);
        Assert.Equal(2, result.History.Count(h => h.Type == ActivityType.StatusChanged));
        Assert.Single(result.History, h => h.Type == ActivityType.NotificationRequested);
    }

    [Fact]
    public void RegisterPayment_FullPayment_RecordsThePaymentAndDelivers()
    {
        // Arrange
        var order = _app.AddOrderReadyForPickup();

        // Act
        var result = _service.RegisterPayment(order.Id, Examples.PaymentAmount, Examples.TransactionReference, _app.Employee);

        // Assert
        var payment = Assert.Single(result.Payments);
        Assert.Equal(Examples.TransactionReference, payment.TransactionReference);
        Assert.Equal(0m, result.OutstandingBalance);
        Assert.Equal(OrderStatus.Delivered, result.Status);
        Assert.Single(result.History, h => h.Type == ActivityType.PaymentReceived);
    }

    // Nothing is paid yet, so the balance is the agreed price: zero for a free order (§4 Calculated values).
    public static TheoryData<decimal> AgreedPrices => new() { 0m, Examples.AgreedPrice };

    [Theory]
    [MemberData(nameof(AgreedPrices))]
    public void AddOrder_NothingPaid_OutstandingBalanceIsTheAgreedPrice(decimal agreedPrice)
    {
        // Act
        var order = _app.AddOrder(agreedPrice: agreedPrice);

        // Assert
        Assert.Equal(agreedPrice, order.OutstandingBalance);
    }

    [Fact]
    public void MarkDelivered_FreeOrderReadyForPickup_SetsDelivered()
    {
        // Arrange
        var order = _app.AddOrderReadyForPickup(agreedPrice: 0m);

        // Act
        var result = _service.MarkDelivered(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.Delivered, result.Status);
        Assert.Equal(3, result.History.Count(h => h.Type == ActivityType.StatusChanged));
    }

    [Fact]
    public void ListOrders_NoFilter_ReturnsAll()
    {
        // Arrange
        _app.AddOrder();
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(null, null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ListOrders_WithStatus_ReturnsOnlyMatching()
    {
        // Arrange
        _app.AddOrderInProgress();
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(OrderStatus.InProgress, null);

        // Assert
        var found = Assert.Single(result);
        Assert.Equal(OrderStatus.InProgress, found.Status);
    }

    [Fact]
    public void ListOrders_WithCustomerId_ReturnsOnlyMatching()
    {
        // Arrange
        var customer = _app.AddCustomer();
        _app.AddOrder(customer.Id);
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(null, customer.Id);

        // Assert
        var found = Assert.Single(result);
        Assert.Equal(customer.Id, found.CustomerId);
    }

    [Fact]
    public void GetOrder_TwoActions_ReturnsTheHistoryOldestFirstWithTheTime()
    {
        // Arrange: the order is made now, and started an hour later
        var createdAt = _app.Clock.Now;
        var order = _app.AddOrder();
        _app.Clock.Advance(TimeSpan.FromHours(1));
        _service.StartOrder(order.Id, _app.Employee);

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Collection(found.History,
            entry =>
            {
                Assert.Equal(ActivityType.OrderCreated, entry.Type);
                Assert.Equal(createdAt, entry.OccurredAt);
            },
            entry =>
            {
                Assert.Equal(ActivityType.StatusChanged, entry.Type);
                Assert.Equal(createdAt.AddHours(1), entry.OccurredAt);
            });
    }
}
