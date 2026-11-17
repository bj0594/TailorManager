using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders CustomersPage with the fake client, so the tests need no API.
public class CustomersPageTests : BunitContext
{
    private readonly FakeCustomersClient _client = new ();

    public CustomersPageTests()
{
    Services.AddSingleton<ICustomersClient>(_client);
}

    [Fact]
    public void CustomerDetailsPage_ExistingId_ShowsIt()
    {
        // Arrange
        var customer = new CustomerDto(Guid.NewGuid(), "Anna Hansen", "98765432", "anna@example.com", "Storgata 1, 0155 Oslo");
        _client.Items.Add(customer);

        // Act
        var page = Render<CustomerDetailsPage> (p => p.Add(x => x.Id, customer.Id));

        // Assert
        Assert.Contains("Anna Hansen", page.Find("h1").TextContent);
    }
}
