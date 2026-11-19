using TailorManager.Core;

namespace TailorManager.Tests;

public class CustomerServiceTests
{
    // The services with fake repositories (TestData/FakeServices.cs).
    private readonly FakeServices _app = new();
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _service = _app.Customers;
    }

    [Fact]
    public void AddCustomer_ValidInput_ReturnsTheCustomer()
    {
        // Act
        var customer = _service.AddCustomer(Examples.CustomerName, Examples.CustomerPhone, Examples.CustomerEmail, Examples.CustomerAddress);

        // Assert
        Assert.Equal(Examples.CustomerName, customer.Name);
    }

    [Fact]
    public void GetCustomer_ExistingId_ReturnsThatCustomer()
    {
        // Arrange
        _app.AddCustomer(Examples.OtherCustomerName);
        var customer = _app.AddCustomer();

        // Act
        var found = _service.GetCustomer(customer.Id);

        // Assert
        Assert.Equal(customer.Id, found.Id);
    }
}
