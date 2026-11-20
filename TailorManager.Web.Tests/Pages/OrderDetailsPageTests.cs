using Bunit;
using TailorManager.Web.Components.Pages;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Renders OrderDetailsPage with the fake orderss from PageTestContext, so the tests need no API.
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
        _tokens.Role = Roles.Workshop;
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

    [Fact]
    public void OrderDetailsPage_StartRejected_ShowsMessage()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        _orders.Items.Add(order);
        _orders.Fail = new HttpRequestException("The order is not Submitted.");
        _tokens.Role = Roles.Workshop;
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Act
        page.Find("button.start").Click();

        // Assert
        page.WaitForAssertion(() => Assert.Contains("The order is not Submitted.", page.Find(".error").TextContent));
    }

    [Fact]
    public void OrderDetailsPage_SubmitWorkNote_SendsItToTheApi()
    {
        // Arrange
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        _orders.Items.Add(order);
        var page = Render<OrderDetailsPage> (p => p.Add(x => x.Id, order.Id));

        // Act
        page.Find("form.note input").Change(Examples.WorkNote);
        page.Find("form.note").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Contains((order.Id, Examples.WorkNote), _orders.AddedWorkNotes));
    }

    [Fact]
    public void OrderDetailsPage_NotWorkshop_HidesWorkNote()
    {
        // Arrange
        _tokens.Role = Roles.FrontDesk;
        var order = TestDtos.NewOrder() with { Status = "InProgress" };
        _orders.Items.Add(order);

        // Act
        var page = Render <OrderDetailsPage > (p => p.Add(x => x.Id, order.Id));

        // Assert
        Assert.Empty(page.FindAll("button.worknote"));
    }
}
