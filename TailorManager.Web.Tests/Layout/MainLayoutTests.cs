using Bunit;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Layout;

namespace TailorManager.Web.Tests;

// The menu shows each part only its own pages (Menu: in §4).
// The TokenStore is _tokens from PageTestContext.
public class MainLayoutTests : PageTestContext
{
    [Fact]
    public void MainLayout_FrontDesk_LinksOnlyTheirPages()
    {
        // Arrange
        _tokens.Role = Roles.FrontDesk;

        // Act
        var layout = Render<MainLayout>(p => p.Add(l => l.Body, "<p>page</p>"));

        // Assert
        var links = layout.FindAll("nav a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("front-desk/orders", links);
        Assert.DoesNotContain("workshop/orders", links);
    }

    [Fact]
    public void MainLayout_Workshop_LinksOnlyTheirPages()
    {
        // Arrange
        _tokens.Role = Roles.Workshop;

        // Act
        var layout = Render<MainLayout>(p => p.Add(l => l.Body, "<p>page</p>"));

        // Assert
        var links = layout.FindAll("nav a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("workshop/orders", links);
        Assert.DoesNotContain("front-desk/orders", links);
    }

    [Fact]
    public void MainLayout_LoggedOut_LinksNoPages()
    {
        // Act
        var layout = Render<MainLayout>(p => p.Add(l => l.Body, "<p>page</p>"));

        // Assert
        Assert.Empty(layout.FindAll("nav a"));
    }
}
