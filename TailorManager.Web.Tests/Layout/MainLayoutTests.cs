using Bunit;
using TailorManager.Web.Components.Layout;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// The menu links every page in Menu: in §4.
public class MainLayoutTests : BunitContext
{
    private readonly TokenStore _tokens = new();

    public MainLayoutTests()
    {
        Services.AddSingleton(_tokens);
    }

    [Fact]
    public void MainLayout_FrontDesk_LinksOnlyTheirPages()
    {
        // Arrange
        _tokens.Role = "FrontDesk";

        // Act
        var layout = Render<MainLayout>(p => p.Add(l => l.Body, "<p>page</p>"));

        // Assert
        var links = layout.FindAll("nav a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("front-desk/orders", links);
        Assert.DoesNotContain("workshop/orders", links);
    }
}
