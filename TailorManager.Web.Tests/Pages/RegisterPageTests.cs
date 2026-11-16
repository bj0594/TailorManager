using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Web.Api;
using TailorManager.Web.Components.Pages;

namespace TailorManager.Web.Tests;

// Renders RegisterPage with the fake auth client.
public class RegisterPageTests : BunitContext
{
    private readonly FakeAuthClient _auth = new();

    public RegisterPageTests()
    {
        Services.AddSingleton<IAuthClient>(_auth);
    }

    [Fact]
    public void RegisterPage_UsernameTaken_ShowsError()
    {
        // Arrange
        _auth.Succeeds = false;
        var page = Render<RegisterPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.NotNull(page.Find(".error")));
    }

    [Fact]
    public void RegisterPage_NewUser_GoesToLogin()
    {
        // Arrange
        _auth.Succeeds = true;
        var page = Render<RegisterPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        var navigation = Services.GetRequiredService<NavigationManager>();
        page.WaitForAssertion(() => Assert.EndsWith("/login", navigation.Uri));
    }

    [Fact]
    public void RegisterPage_ApiRejects_ShowsTheMessage()
    {
        // Arrange
        _auth.Succeeds = false;
        _auth.Error = "Unknown role: Janitor.";
        var page = Render<RegisterPage>();

        // Act
        page.Find("form").Submit();

        // Assert
        page.WaitForAssertion(() => Assert.Equal("Unknown role: Janitor.", page.Find(".error").TextContent));
    }
}
