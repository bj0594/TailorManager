using Bunit;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders CustomerDetailsPage with the fake clients from PageTestContext, so the tests need no API.
public class CustomerDetailsPageTests : PageTestContext
{
    [Fact]
    public void CustomerDetailsPage_ExistingId_ShowsIt()
    {
        // Arrange
        var customer = TestDtos.NewCustomer();
        _customers.Items.Add(customer);

        // Act
        var page = Render<CustomerDetailsPage>(p => p.Add(x => x.Id, customer.Id));

        // Assert
        Assert.Contains(Examples.CustomerName, page.Find("h1").TextContent);
    }
}
