using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders LoginPage with the fake auth client.
public class LoginPageTests : BunitContext
{
    private readonly FakeAuthClient _auth = new();
    private readonly TokenStore _tokens = new();

    public LoginPageTests()
    {
        Services.AddSingleton<IAuthClient>(_auth);
        Services.AddSingleton(_tokens);
    }

    [Fact]
    public void LoginPage_WrongPassword_ShowsError()
    {
        // Arrange
        _auth.Succeeds = false;
        var page = Render<LoginPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.NotNull(page.Find(".error")));
    }

    [Fact]
    public void LoginPage_WorkshopUser_GoesToTheirPage()
    {
        // Arrange
        _auth.Succeeds = true;
        _tokens.Role = Roles.Workshop;
        var page = Render<LoginPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        var navigation = Services.GetRequiredService<NavigationManager>();
        page.WaitForAssertion(() => Assert.EndsWith("/workshop/orders", navigation.Uri));
    }

    [Fact]
    public void LoginPage_FrontDeskUser_GoesToTheirPage()
    {
        // Arrange
        _auth.Succeeds = true;
        _tokens.Role = Roles.FrontDesk;
        var page = Render<LoginPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        var navigation = Services.GetRequiredService<NavigationManager>();
        page.WaitForAssertion(() => Assert.EndsWith("/front-desk", navigation.Uri));
    }
}
