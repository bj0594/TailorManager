using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders OrdersPage with the fake client, so the tests need no API.
public class OrderDetailsPageTests : BunitContext
{
    private readonly FakeOrdersClient _client = new ();

    public OrderDetailsPageTests()
    {
        Services.AddSingleton<IOrdersClient>(_client);
    }

    [Fact]
    public void OrderDetailsPage_ExistingId_ShowsIt()
    {
        // Arrange
        var order = new OrderDto(Guid.NewGuid(), Guid.NewGuid(), "NewGarment", "Shorten the trousers by 3 cm", 450m, 450m, 450m, "Submitted", DateTimeOffset.Now, null);
        _client.Items.Add(order);

        // Act
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Assert
        Assert.Contains("Shorten the trousers by 3 cm", page.Find("h1").TextContent);
    }

    [Fact]
    public void OrderDetailsPage_ClickStart_SendsItToTheApi()
    {
        // Arrange
        var order = new OrderDto(Guid.NewGuid(), Guid.NewGuid(), "NewGarment", "Shorten the trousers by 3 cm", 450m, 450m, 450m, "Submitted", DateTimeOffset.Now, null);
        _client.Items.Add(order);
        var page = Render<OrderDetailsPage>(p => p.Add(x => x.Id, order.Id));

        // Act
        page.Find("button.start").Click();

        // Assert
        page.WaitForAssertion(() => Assert.Contains(order.Id, _client.StartIds));
    }
}
