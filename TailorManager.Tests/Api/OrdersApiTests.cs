using System.Net;
using System.Net.Http.Json;
using TailorManager.Core;
using Microsoft.AspNetCore.Mvc.Testing;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class OrdersApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new(); private readonly HttpClient _client;

    public OrdersApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostOrders_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var client = await AuthHelper.CreateLoggedInClientAsync(_factory, "kari", Roles.FrontDesk);
        var created = await client.PostAsJsonAsync("/customers", new { name = "Anna Hansen", phone = "98765432", email = "anna@example.com", address = "Storgata 1, 0155 Oslo" });
        var customer = await created.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await client.PostAsJsonAsync("/orders", new { customerId = customer!.Id, type = "NewGarment", workDescription = "Shorten the trousers by 3 cm", agreedPrice = 450, relatedOrderId = (Guid?)null });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(customer.Id, body!.CustomerId);
    }

    [Fact]
    public async Task PostOrders_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/orders", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostOrders_NotFrontDesk_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var user = await AuthHelper.CreateLoggedInClientAsync(_factory, "user", Roles.Workshop);

        // Act
        var response = await user.PostAsJsonAsync("/orders", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private record OrderResponse(Guid Id, Guid CustomerId, string WorkDescription, decimal InitialAgreedPrice);
    private record CustomerResponse(Guid Id, string Name, string Email, string Phone, string Address);
}
