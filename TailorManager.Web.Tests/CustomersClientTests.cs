using System.Net;
using System.Text.Json;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Tests the real client against answers the test gives it, so no API has to run.
public class CustomersClientTests
{
    private readonly StubHttpMessageHandler _handler = new();
    private readonly CustomersClient _client;

    public CustomersClientTests()
    {
        _client = new CustomersClient(new HttpClient(_handler) { BaseAddress = new Uri("http://api.test/") }, new TokenStore());
    }

    [Fact]
    public async Task GetCustomersAsync_ApiReturnsTwo_ReturnsBoth()
        {
        // Arrange
        _handler.Respond("/customers", JsonSerializer.Serialize(new[] { new { id = Guid.NewGuid() }, new { id = Guid.NewGuid() } }));

        // Act
        var result = await _client.GetCustomersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/customers", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetCustomersAsync_LoggedIn_SendsTheToken()
{
        // Arrange
        var client = new CustomersClient(new HttpClient(_handler) { BaseAddress = new Uri("http://api.test/") }, new TokenStore { Token = "abc" });
        _handler.Respond("/Customers", "[]");

        // Act
        await client.GetCustomersAsync();

        // Assert
        Assert.Equal("Bearer abc", _handler.Requests.Single().Headers.Authorization?.ToString());
    }
}
