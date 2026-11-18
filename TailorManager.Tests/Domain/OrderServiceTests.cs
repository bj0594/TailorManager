using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public class OrderServiceTests
{
    private readonly FakeCustomerRepository _customerRepository = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = new(new FakeOrderRepository(), _customerRepository);
    }

    [Fact]
    public void CreateOrder_ValidInput_ReturnsTheOrder()
    {
        // Arrange
        var customer = new CustomerService(_customerRepository).AddCustomer("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");
        var workDescription = "Shorten the trousers by 3 cm";

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, workDescription, 450m, null, new User());

        // Assert
        Assert.Equal(workDescription, order.WorkDescription);
    }

    [Fact]
    public void AddOrder_WithEmployee_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var customer = new CustomerService(_customerRepository).AddCustomer("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");
        var employee = new User { Id = Guid.NewGuid(), FirstName = "Kari", LastName = "Nordmann" };

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, "Shorten the trousers by 3 cm", 450m, null, employee);

        // Assert
        Assert.Equal("Kari N.", order.History.First().ActorDisplayName);
    }

    [Fact]
    public void GetOrder_ExistingId_ReturnsThatOrder()
    {
        // Arrange
        var customer = new CustomerService(_customerRepository).AddCustomer("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");
        var employee = new User { Id = Guid.NewGuid(), FirstName = "Kari", LastName = "Nordmann" };
        _service.AddOrder(customer.Id, OrderType.NewGarment, "Take in the waist by 2 cm", 450m, null, employee);
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, "Shorten the trousers by 3 cm", 450m, null, employee);

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Equal(order.Id, found.Id);
    }
}
