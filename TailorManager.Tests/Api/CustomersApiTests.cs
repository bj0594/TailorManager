using System.Net;
using System.Net.Http.Json;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class CustomersApiTests : ApiTestBase
{
    [Fact]
    public async Task PostCustomers_ValidCustomer_ReturnsCreated()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();

        // Act
        var response = await client.PostAsJsonAsync("/customers", ApiTestData.NewCustomerRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.Equal(Examples.CustomerName, body?.Name);
    }

    [Fact]
    public async Task PostCustomers_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/customers", ApiTestData.NewCustomerRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCustomers_NotFrontDesk_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var user = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await user.PostAsJsonAsync("/customers", ApiTestData.NewCustomerRequest());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ExistingId_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.GetAsync($"/customers/{customer.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.Equal(customer.Id, body!.Id);
    }

    [Fact]
    public async Task GetCustomers_WithSearchTerm_ReturnsOnlyMatching()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var match = await client.CreateCustomerAsync();
        await client.CreateCustomerAsync(Examples.OtherCustomerName);

        // Act
        var response = await client.GetAsync($"/customers?searchTerm={Uri.EscapeDataString(Examples.CustomerName)}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
        Assert.Equal(match.Id, Assert.Single(body!).Id);
    }
}
