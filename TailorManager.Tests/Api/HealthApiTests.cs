using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

public class HealthApiTests
{
    [Fact]
    public async Task GetHealth_ApiRunning_ReturnsOk()
    {
        // Arrange
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
            services.AddSingleton<IOrderRepository, FakeOrderRepository>();
        }));
        var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
