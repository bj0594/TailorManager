using Bunit;
using TailorManager.Web.Components.Pages.Workshop;

namespace TailorManager.Web.Tests;

// Renders WorkshopOrdersPage with the fake clients from PageTestContext, so the tests need no API.
public class WorkshopOrdersPageTests : PageTestContext
{
    [Fact]
    public void WorkshopOrdersPage_FilterByStatus_ShowsOnlyMatching()
    {
        // Arrange
        _orders.Items.Add(TestDtos.NewOrder() with { Status = "InProgress" });
        _orders.Items.Add(TestDtos.NewOrder(workDescription: Examples.OtherWorkDescription));
        var page = Render<WorkshopOrdersPage>();

        // Act
        page.Find("#status").Change("InProgress");
        page.Find("#search-button").Click();

        // Assert
        page.WaitForAssertion(() =>
            Assert.Contains(Examples.WorkDescription, Assert.Single(page.FindAll("li")).TextContent));
    }

    [Fact]
    public void WorkshopOrdersPage_NoOrders_ShowsNoOrdersYet()
    {
        // Act
        var page = Render<WorkshopOrdersPage>();

        // Assert
        page.WaitForAssertion(() => Assert.Contains("No orders yet", page.Find(".empty").TextContent));
    }

    [Fact]
    public void WorkshopOrdersPage_ApiDoesNotAnswer_ShowsCouldNotLoadOrders()
    {
        // Arrange
        _orders.LoadFail = new HttpRequestException("No connection could be made.");

        // Act
        var page = Render<WorkshopOrdersPage>();

        // Assert
        page.WaitForAssertion(() => Assert.Contains("Could not load orders", page.Find(".error").TextContent));
    }
}
