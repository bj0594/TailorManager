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
    public async Task FindCustomersAsync_ApiReturnsTwo_ReturnsBoth()
    {
        // Arrange
        _handler.Respond("/customers", TestDtos.ToJson(new[] { TestDtos.NewCustomer(), TestDtos.NewCustomer(Examples.OtherCustomerName) }));

        // Act
        var result = await _client.FindCustomersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        _handler.AssertSent(HttpMethod.Get, "/customers");
    }

    [Fact]
    public async Task FindCustomersAsync_LoggedIn_SendsTheToken()
    {
        // Arrange
        var client = new CustomersClient(_handler.CreateHttpClient(), new TokenStore { Token = "abc" });
        _handler.Respond("/customers", "[]");

        // Act
        await client.FindCustomersAsync();

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
        _handler.AssertSent(HttpMethod.Post, "/customers");
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
        _handler.AssertSent(HttpMethod.Get, $"/customers/{customer.Id}");
        Assert.Equal(Examples.CustomerName, result.Name);
    }
}
