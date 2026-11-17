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

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostCustomers_ValidCustomer_ReturnsCreated()
    {
        // Arrange
        var client = await AuthHelper.CreateLoggedInClientAsync(_factory, "owner", "FrontDesk");

        // Act
        var response = await client.PostAsJsonAsync("/customers", new {
            Name = "John",
            Email = "john@john.com",
            PhoneNumber = "12345678",
            Address = "Test Street 1"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync <CustomerResponse>();
        Assert.Equal("John", body?.Name);
    }

    [Fact]
    public async Task PostCustomers_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/customers", new
        {
            Name = "Anna Hansen",
            Email = "anna@example.com",
            PhoneNumber = "98765432",
            Address = "Storgata 1, 0155 Oslo"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostCustomers_NotFrontDesk_ReturnsForbidden()
{
        // Arrange: a logged-in user without the role
        var user = await AuthHelper.CreateLoggedInClientAsync(_factory, "user", "Workshop");

        // Act
        var response = await user.PostAsJsonAsync("/customers", new
        {
            Name = "Anna Hansen",
            Email = "anna@example.com",
            PhoneNumber = "98765432",
            Address = "Storgata 1, 0155 Oslo"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
    private record CustomerResponse(Guid Id, string Name, string Email, string PhoneNumber, string Address);
}
