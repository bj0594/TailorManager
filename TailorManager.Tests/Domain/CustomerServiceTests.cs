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

    [Fact]
    public void GetCustomer_ExistingId_ReturnsThatCustomer()
{
        // Arrange
        _service.AddCustomer("Ola Nordmann", "12345678", "ola@example.com", "Kirkegata 2, 0153 Oslo");
        var customer = _service.AddCustomer("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");

        // Act
        var found = _service.GetCustomer(customer.Id);

        // Assert
        Assert.Equal(customer.Id, found.Id);
    }
}
