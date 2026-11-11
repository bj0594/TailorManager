using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public class CustomerServiceTests
{
    private readonly CustomerService _service = new(new FakeCustomerRepository());

    [Fact]
    public void AddCustomer_ValidInput_ReturnsTheCustomer()
    {
        // Arrange
        var name = "Anna Hansen";

        // Act
        var customer = _service.AddCustomer(name, "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");

        // Assert
        Assert.Equal(name, customer.Name);
    }

}
