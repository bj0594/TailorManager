using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repositories, so the API tests need no database.
// Every test class creates its own, so each test starts with empty repositories.
public class TestApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<ICustomerRepository, FakeCustomerRepository>();
            services.AddSingleton<IOrderRepository, FakeOrderRepository>();
            services.AddSingleton<IUserRepository, FakeUserRepository>();
        });
    }
}
