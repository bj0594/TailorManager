using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public class OrderServiceTests
{
    private const string WorkDescription = "Shorten the trousers by 3 cm";

    private readonly OrderService _service;
    private readonly Customer _customer;
    private readonly User _employee = new() { Id = Guid.NewGuid(), FirstName = "Kari", LastName = "Nordmann" };

    public OrderServiceTests()
    {
        var customerRepository = new FakeCustomerRepository();
        _service = new(new FakeOrderRepository(), customerRepository);
        _customer = new CustomerService(customerRepository).AddCustomer("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");
    }

    [Fact]
    public void AddOrder_ValidInput_ReturnsTheOrder()
    {
        // Act
        var order = AddOrder();

        // Assert
        Assert.Equal(WorkDescription, order.WorkDescription);
    }

    [Fact]
    public void AddOrder_WithEmployee_StoresTheEmployeeInTheHistory()
    {
        // Act
        var order = AddOrder();

        // Assert
        Assert.Equal("Kari N.", order.History.First().ActorDisplayName);
    }

    [Fact]
    public void GetOrder_ExistingId_ReturnsThatOrder()
    {
        // Arrange
        AddOrder("Take in the waist by 2 cm");
        var order = AddOrder();

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Equal(order.Id, found.Id);
    }

    [Fact]
    public void StartOrder_ExistingOrder_SetsInProgress()
    {
        // Arrange
        var order = AddOrder();

        // Act
        _service.StartOrder(order.Id);

        // Assert
        Assert.Equal(OrderStatus.InProgress, _service.GetOrder(order.Id).Status);
    }

    private Order AddOrder(string workDescription = WorkDescription) =>
        _service.AddOrder(_customer.Id, OrderType.NewGarment, workDescription, 450m, null, _employee);
}
