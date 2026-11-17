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
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, workDescription, 450m, null);

        // Assert
        Assert.Equal(workDescription, order.WorkDescription);
    }
}
