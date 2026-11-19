using System.Net;
using System.Net.Http.Json;
using TailorManager.Core;

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
        var client = await LogInAsFrontDeskAsync();
        var customer = await CreateCustomerAsync(client);

        // Act
        var response = await PostOrderAsync(client, customer.Id);

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

    [Fact]
    public async Task PostOrders_LoggedIn_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var client = await LogInAsFrontDeskAsync();
        var customer = await CreateCustomerAsync(client);

        // Act
        var response = await PostOrderAsync(client, customer.Id);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("Kari N.", body!.History[0].ActorDisplayName);
    }

    [Fact]
    public async Task GetOrder_ExistingId_ReturnsOk()
    {
        // Arrange
        var client = await LogInAsFrontDeskAsync();
        var order = await CreateOrderAsync(client);

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
        var client = await LogInAsFrontDeskAsync();
        var order = await CreateOrderAsync(client);

        // Act
        var response = await client.PatchAsync($"/orders/{order.Id}/start", null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    private record OrderResponse(Guid Id, Guid CustomerId, string WorkDescription, decimal InitialAgreedPrice, List<OrderHistoryResponse> History);
    private record OrderHistoryResponse(string? ActorDisplayName);
    private record CustomerResponse(Guid Id, string Name, string Email, string Phone, string Address);

    private Task<HttpClient> LogInAsFrontDeskAsync() =>
        AuthHelper.CreateLoggedInClientAsync(_factory, "kari", Roles.FrontDesk);

    private static async Task<CustomerResponse> CreateCustomerAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/customers", new { name = "Anna Hansen", phone = "98765432", email = "anna@example.com", address = "Storgata 1, 0155 Oslo" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    private static Task<HttpResponseMessage> PostOrderAsync(HttpClient client, Guid customerId) =>
        client.PostAsJsonAsync("/orders", new { customerId, type = "NewGarment", workDescription = "Shorten the trousers by 3 cm", agreedPrice = 450, relatedOrderId = (Guid?)null });

    private static async Task<OrderResponse> CreateOrderAsync(HttpClient client)
    {
        var customer = await CreateCustomerAsync(client);
        var response = await PostOrderAsync(client, customer.Id);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }
}
