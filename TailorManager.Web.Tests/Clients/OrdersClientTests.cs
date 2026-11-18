using System.Text.Json;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Tests the real client against answers the test gives it, so no API has to run.
public class OrdersClientTests
{
    private readonly StubHttpMessageHandler _handler = new();
    private readonly OrdersClient _client;

    public OrdersClientTests()
    {
        _client = CreateClient(new TokenStore());
    }

    [Fact]
    public async Task GetOrdersAsync_ApiReturnsTwo_ReturnsBoth()
    {
        // Arrange
        _handler.Respond("/orders", JsonSerializer.Serialize(new[] { new { id = Guid.NewGuid() }, new { id = Guid.NewGuid() } }));

        // Act
        var result = await _client.GetOrdersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/orders", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetOrdersAsync_LoggedIn_SendsTheToken()
    {
        // Arrange
        var client = CreateClient(new TokenStore { Token = "abc" });
        _handler.Respond("/orders", "[]");

        // Act
        await client.GetOrdersAsync();

        // Assert
        Assert.Equal("Bearer abc", _handler.Requests.Single().Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetOrderAsync_ExistingId_ReadsIt()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}", JsonSerializer.Serialize(new { id, workDescription = "Shorten the trousers by 3 cm" }));

        // Act
        var result = await _client.GetOrderAsync(id);

        // Assert
        Assert.Equal(HttpMethod.Get, _handler.Requests.Single().Method);
        Assert.Equal("Shorten the trousers by 3 cm", result.WorkDescription);
    }

    // A client that talks to the stub instead of the API.
    private OrdersClient CreateClient(TokenStore tokens) =>
        new(new HttpClient(_handler) { BaseAddress = new Uri("http://api.test/") }, tokens);
}
