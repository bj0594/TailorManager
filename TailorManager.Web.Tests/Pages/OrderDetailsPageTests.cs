using Bunit;
using TailorManager.Web.Components.Pages;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Renders OrderDetailsPage with the fake clients from PageTestContext, so the tests need no API.
public class OrderDetailsPageTests : PageTestContext
{
    [Fact]
    public void OrderDetailsPage_ExistingId_ShowsIt()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        _orders.Items.Add(order);

        // Act
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Assert
        Assert.Contains(Examples.WorkDescription, page.Find("h1").TextContent);
    }

    [Fact]
    public void OrderDetailsPage_ClickStart_SendsItToTheApi()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        _orders.Items.Add(order);
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Act
        page.Find("button.start").Click();

        // Assert
        page.WaitForAssertion(() => Assert.Contains(order.Id, _orders.StartIds));
    }

    [Fact]
    public void OrderDetailsPage_NotWorkshop_HidesTheStartButton()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        _orders.Items.Add(order);
        _tokens.Role = Roles.FrontDesk;

        // Act
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Assert
        Assert.Empty(page.FindAll("button.start"));
    }
}
