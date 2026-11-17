using System.Text.Json;
using TailorManager.Web.Api;
using System.Net;

namespace TailorManager.Web.Tests;

// Tests the real client against answers the test gives it, so no API has to run.
public class CustomersClientTests
{
    private readonly StubHttpMessageHandler _handler = new();
    private readonly CustomersClient _client;

    public CustomersClientTests()
    {
        _client = CreateClient(new TokenStore());
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
        var client = CreateClient(new TokenStore { Token = "abc" });
        _handler.Respond("/customers", "[]");

        // Act
        await client.GetCustomersAsync();

        // Assert
        Assert.Equal("Bearer abc", _handler.Requests.Single().Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task AddCustomerAsync_Valid_PostsTheFields()
{
        // Arrange
        _handler.Respond("/customers", "{}", HttpStatusCode.Created);

        // Act
        await _client.AddCustomerAsync("Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/customers", request.RequestUri!.AbsolutePath);
        Assert.Equal("Anna Hansen", JsonDocument.Parse(_handler.Bodies.Single()!).RootElement.GetProperty("name").GetString());
    }

    [Fact]
    public async Task GetCustomerAsync_ExistingId_ReadsIt()
{
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/customers/{id}", JsonSerializer.Serialize(new {id, 
            name = "Anna Hansen",
            phone = "98765432",
            email = "anna@example.com",
            address = "Storgata 1, 0155 Oslo"
            }));

        // Act
        var result = await _client.GetCustomerAsync(id);

        // Assert
        Assert.Equal(HttpMethod.Get, _handler.Requests.Single().Method);
        Assert.Equal("Anna Hansen", result.Name);
    }

    // A client that talks to the stub instead of the API.
    private CustomersClient CreateClient(TokenStore tokens) =>
        new(new HttpClient(_handler) { BaseAddress = new Uri("http://api.test/") }, tokens);
}
