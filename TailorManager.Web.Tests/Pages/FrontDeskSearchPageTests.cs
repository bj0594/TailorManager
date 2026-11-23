using Bunit;
using TailorManager.Web.Components.Pages.FrontDesk;

namespace TailorManager.Web.Tests;

// Renders FrontDeskSearchPage with the fake clients from PageTestContext, so the tests need no API.
public class FrontDeskSearchPageTests : PageTestContext
{
    [Fact]
    public void FrontDeskSearchPage_AddCustomer_SendsItToTheApi()
    {
        // Arrange
        var page = Render<FrontDeskSearchPage>();

        // Act
        page.Find("#name").Change(Examples.CustomerName);
        page.Find("#email").Change(Examples.CustomerEmail);
        page.Find("#phone").Change(Examples.CustomerPhone);
        page.Find("#address").Change(Examples.CustomerAddress);
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Single(_customers.Items));
    }

    [Fact]
    public void FrontDeskSearchPage_AddCustomer_ShowsItInTheResults()
    {
        // Arrange
        var page = Render<FrontDeskSearchPage>();

        // Act
        page.Find("#name").Change(Examples.CustomerName);
        page.Find("#phone").Change(Examples.CustomerPhone);
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Contains(Examples.CustomerName, page.Find(".results").TextContent));
    }
}
