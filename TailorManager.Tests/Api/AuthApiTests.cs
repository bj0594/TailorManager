using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public class AuthApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
                services.AddSingleton<IOrderRepository, FakeOrderRepository>();
                // new: users are stored in memory in the tests
                services.AddSingleton<IUserRepository, FakeUserRepository>();
            }));
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostAuthRegister_NewUser_ReturnsCreated()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", new { Username = "employee", Password = "Test-password-1" });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_TakenUsername_ReturnsConflict()
    {
        // Arrange
        var credentials = new { Username = "owner", Password = "Test-password-1" };
        await _client.PostAsJsonAsync("/auth/register", credentials);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", credentials);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
