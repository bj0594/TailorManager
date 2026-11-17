using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class CustomersApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new(); private readonly HttpClient _client;

    public CustomersApiTests()
    {
        _client = _factory.CreateClient();
    }

    // The customer from the Example line in §4, as the API gets it in POST /customers.
    private static readonly object NewCustomer = new
    {
        name = "Anna Hansen",
        phone = "98765432",
        email = "anna@example.com",
        address = "Storgata 1, 0155 Oslo"
    };
    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostCustomers_ValidCustomer_ReturnsCreated()
    {
        // Arrange
        var client = await AuthHelper.CreateLoggedInClientAsync(_factory, "owner", "FrontDesk");

        // Act
        var response = await client.PostAsJsonAsync("/customers", NewCustomer);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync <CustomerResponse>();
        Assert.Equal("Anna Hansen", body?.Name);
    }

    [Fact]
    public async Task PostCustomers_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/customers", NewCustomer);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCustomers_NotFrontDesk_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var user = await AuthHelper.CreateLoggedInClientAsync(_factory, "user", "Workshop");

        // Act
        var response = await user.PostAsJsonAsync("/customers", NewCustomer);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetCustomer_ExistingId_ReturnsOk()
{
        // Arrange
        var created = await _client.PostAsJsonAsync("/customers", NewCustomer);
        var customer = await created.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync($"/customers/{customer !.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        Assert.Equal(customer.Id, body!.Id);
    }
    private record CustomerResponse(Guid Id, string Name, string Email, string Phone, string Address);
}
