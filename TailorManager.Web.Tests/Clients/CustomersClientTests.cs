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
        _client = new CustomersClient(_handler.CreateHttpClient(), new TokenStore());
    }

    [Fact]
    public async Task GetCustomersAsync_ApiReturnsTwo_ReturnsBoth()
    {
        // Arrange
        _handler.Respond("/customers", TestDtos.ToJson(new[] { TestDtos.NewCustomer(), TestDtos.NewCustomer(Examples.OtherCustomerName) }));

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
        var client = new CustomersClient(_handler.CreateHttpClient(), new TokenStore { Token = "abc" });
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
        _handler.Respond("/customers", TestDtos.ToJson(TestDtos.NewCustomer()), HttpStatusCode.Created);

        // Act
        var result = await _client.AddCustomerAsync(Examples.CustomerName, Examples.CustomerPhone, Examples.CustomerEmail, Examples.CustomerAddress);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("/customers", request.RequestUri!.AbsolutePath);
        Assert.Equal(Examples.CustomerName, JsonDocument.Parse(_handler.Bodies.Single()!).RootElement.GetProperty("name").GetString());
        Assert.Equal(Examples.CustomerName, result.Name);
    }

    [Fact]
    public async Task GetCustomerAsync_ExistingId_ReadsIt()
    {
        // Arrange
        var customer = TestDtos.NewCustomer();
        _handler.Respond($"/customers/{customer.Id}", TestDtos.ToJson(customer));

        // Act
        var result = await _client.GetCustomerAsync(customer.Id);

        // Assert
        Assert.Equal(HttpMethod.Get, _handler.Requests.Single().Method);
        Assert.Equal(Examples.CustomerName, result.Name);
    }
}
