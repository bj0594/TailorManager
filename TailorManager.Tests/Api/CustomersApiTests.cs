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
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CustomersApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddSingleton < ICustomerRepository, FakeCustomerRepository > ()));
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostCustomers_ValidCustomer_ReturnsCreated()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/customers", new {
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
    private record CustomerResponse(Guid Id, string Name, string Email, string PhoneNumber, string Address);
}
