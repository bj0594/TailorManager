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

    [Theory]
    [InlineData(Examples.CustomerName)]
    [InlineData(Examples.CustomerPhone)]
    [InlineData(Examples.CustomerEmail)]
    public void FindCustomers_ByNamePhoneOrEmail_ReturnsOnlyMatching(string searchTerm)
    {
        // Arrange
        var match = _app.AddCustomer();
        _service.AddCustomer(Examples.OtherCustomerName, Examples.OtherCustomerPhone, Examples.OtherCustomerEmail, Examples.CustomerAddress);

        // Act
        var result = _service.FindCustomers(searchTerm);

        // Assert
        Assert.Equal(match.Id, Assert.Single(result).Id);
    }

    [Theory]
    [InlineData("name")]
    [InlineData("phone")]
    [InlineData("email")]
    [InlineData("address")]
    public void AddCustomer_MissingField_ThrowsArgumentException(string field)
    {
        // Arrange: the example customer, with one field empty
        var name = field == "name" ? "" : Examples.CustomerName;
        var phone = field == "phone" ? "" : Examples.CustomerPhone;
        var email = field == "email" ? "" : Examples.CustomerEmail;
        var address = field == "address" ? "" : Examples.CustomerAddress;

        // Act + Assert
        var error = Assert.Throws<ArgumentException>(() => _service.AddCustomer(name, phone, email, address));
        Assert.Equal($"The {field} is required.", error.Message);
    }
}
