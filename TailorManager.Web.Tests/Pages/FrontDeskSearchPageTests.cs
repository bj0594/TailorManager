using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages.FrontDesk;

namespace TailorManager.Web.Tests;

public class FrontDeskSearchPageTests : BunitContext
{
    private readonly FakeCustomersClient _client = new();

    public FrontDeskSearchPageTests()
    {
        Services.AddSingleton<ICustomersClient>(_client);
    }

    [Fact]
    public void FrontDeskSearchPage_AddCustomer_SendsItToTheApi()
    {
        // Arrange
        var page = Render<FrontDeskSearchPage>();

        // Act
        page.Find("#name").Change("Anna Hansen");
        page.Find("#email").Change("anna@example.com");
        page.Find("#phone").Change("98765432");
        page.Find("#address").Change("Storgata 1, 0155 Oslo");
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Single(_client.Items));
    }
}
