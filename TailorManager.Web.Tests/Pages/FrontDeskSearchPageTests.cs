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

    [Fact]
    public void FrontDeskSearchPage_Search_ShowsOnlyMatching()
    {
        // Arrange
        _customers.Items.Add(TestDtos.NewCustomer());
        _customers.Items.Add(TestDtos.NewCustomer(Examples.OtherCustomerName));
        var page = Render<FrontDeskSearchPage>();

        // Act
        page.Find("#search").Change(Examples.CustomerName);
        page.Find("#search-button").Click();

        // Assert
        page.WaitForAssertion(() =>
            Assert.Contains(Examples.CustomerName, Assert.Single(page.FindAll(".results li")).TextContent));
    }

    [Fact]
    public void FrontDeskSearchPage_CreateOrder_SendsItToTheApi()
    {
        // Arrange
        var customer = TestDtos.NewCustomer();
        _customers.Items.Add(customer);
        var page = Render<FrontDeskSearchPage>();
        page.Find("#search").Change(Examples.CustomerName);
        page.Find("#search-button").Click();

        // Act
        page.WaitForElement(".results button.neworder").Click();
        page.Find("#type").Change("NewGarment");
        page.Find("#workDescription").Change(Examples.WorkDescription);
        page.Find("#agreedPrice").Change(Examples.AgreedPrice.ToString());
        page.Find("form.order").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Equal(customer.Id, Assert.Single(_orders.Items).CustomerId));
    }

    [Fact]
    public void FrontDeskSearchPage_CreateOrderWithEarlierOrder_SendsTheLink()
    {
        // Arrange: the customer has an earlier order
        var customer = TestDtos.NewCustomer();
        _customers.Items.Add(customer);
        var earlier = TestDtos.NewOrder(customer.Id);
        _orders.Items.Add(earlier);
        var page = Render<FrontDeskSearchPage>();
        page.Find("#search").Change(Examples.CustomerName);
        page.Find("#search-button").Click();

        // Act
        page.WaitForElement(".results button.neworder").Click();
        page.Find("#type").Change("Repair");
        page.Find("#workDescription").Change(Examples.OtherWorkDescription);
        page.Find("#agreedPrice").Change(Examples.AgreedPrice.ToString());
        page.Find("#relatedOrder").Change(earlier.Id.ToString());
        page.Find("form.order").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Equal(earlier.Id, _orders.Items.Last().RelatedOrderId));
    }

    [Fact]
    public void FrontDeskSearchPage_SearchWithoutMatches_ShowsNoCustomersFound()
    {
        // Arrange
        _customers.Items.Add(TestDtos.NewCustomer());
        var page = Render<FrontDeskSearchPage>();

        // Act
        page.Find("#search").Change(Examples.OtherCustomerName);
        page.Find("#search-button").Click();

        // Assert
        page.WaitForAssertion(() => Assert.Contains("No customers found", page.Find(".empty").TextContent));
    }
}
