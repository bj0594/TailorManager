using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// The base class for the page and layout tests. It registers every fake client and the TokenStore,
// so a page that uses one more client (like the order page showing the customer) needs no change in the test class.
//   public class OrderDetailsPageTests : PageTestContext
//   _orders.Items.Add(TestDtos.NewOrder());
public abstract class PageTestContext : BunitContext
{
    protected readonly FakeCustomersClient _customers = new();
    protected readonly FakeOrdersClient _orders = new();
    protected readonly FakeAuthClient _auth = new();
    protected readonly TokenStore _tokens = new();

    protected PageTestContext()
    {
        Services.AddSingleton<ICustomersClient>(_customers);
        Services.AddSingleton<IOrdersClient>(_orders);
        Services.AddSingleton<IAuthClient>(_auth);
        Services.AddSingleton(_tokens);
    }
}
