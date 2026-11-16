using Bunit;
using TailorManager.Web.Components.Layout;

namespace TailorManager.Web.Tests;

// The menu links every page in Menu: in §4.
public class MainLayoutTests : BunitContext
{
    [Fact]
    public void MainLayout_Always_LinksEveryPage()
    {
        // Act
        var layout = Render<MainLayout>(p => p.Add(l => l.Body, "<p>page</p>"));

        // Assert
        var links = layout.FindAll("nav a").Select(a => a.GetAttribute("href")).ToList();
        Assert.Contains("front-desk", links);
        Assert.Contains("front-desk/orders", links);
        Assert.Contains("workshop/orders", links);
        Assert.Contains("login", links);
    }
}
