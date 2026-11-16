using Bunit;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// The start page says what the site is, and links on to the first page.
public class HomeTests : BunitContext
{
    [Fact]
    public void Home_Always_ShowsTheSiteNameAndALinkOn()
    {
        // Act
        var page = Render<Home>();

        // Assert
        Assert.Equal("Tailor Manager", page.Find("h1").TextContent);
        Assert.NotNull(page.Find("a[href='login']"));
        Assert.NotNull(page.Find("a[href='register']"));
    }
}
