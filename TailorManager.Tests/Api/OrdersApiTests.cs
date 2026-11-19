using System.Net;
using System.Net.Http.Json;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class OrdersApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();
    private readonly HttpClient _client;

    public OrdersApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostOrders_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.PostOrderAsync(customer.Id);

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
        var user = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await user.PostAsJsonAsync("/orders", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostOrders_LoggedIn_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.PostOrderAsync(customer.Id);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(Examples.EmployeeDisplayName, body!.History[0].ActorDisplayName);
    }

    [Fact]
    public async Task GetOrder_ExistingId_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();

        // Act
        var response = await client.GetAsync($"/orders/{order.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(order.Id, body!.Id);
    }

    [Fact]
    public async Task PatchOrdersStart_ExistingOrder_ReturnsNoContent()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await workshop.PatchAsync($"/orders/{order.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PatchOrdersStart_NotWorkshop_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var client = await _factory.LogInAsFrontDeskAsync();

        // Act
        var response = await client.PatchAsync($"/orders/{Guid.NewGuid()}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PatchOrdersStart_NotSubmitted_ReturnsConflict()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        await workshop.PatchAsync($"/orders/{order.Id}/start", null);

        // Act
        var response = await workshop.PatchAsync($"/orders/{order.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostOrdersNotes_OrderInProgress_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await client.CreateOrderAsync();
        await workshop.PatchAsync($"/orders/{order.Id}/start", null);

        // Act
        var response = await workshop.PostAsJsonAsync($"/orders/{order.Id}/notes", new { note = Examples.WorkNote });

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Contains(body!.History, h => h.Type == "WorkNoteAdded" && h.Note == Examples.WorkNote);
    }
}
