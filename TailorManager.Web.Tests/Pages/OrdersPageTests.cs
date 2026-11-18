using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders OrdersPage with the fake client, so the tests need no API.
public class OrdersPageTests : BunitContext
{
    private readonly FakeOrdersClient _client = new ();

    public OrdersPageTests()
    {
        Services.AddSingleton<IOrdersClient>(_client);
    }
}
