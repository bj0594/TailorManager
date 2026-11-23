using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// Register, log in and /auth/me, through the API with the fake repositories.
public class AuthApiTests : ApiTestBase
{
    [Fact]
    public async Task PostAuthRegister_NewUser_ReturnsCreated()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", ApiTestData.RegisterRequest("employee"));

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_TakenUsername_ReturnsConflict()
    {
        // Arrange
        var credentials = ApiTestData.RegisterRequest();
        await _client.PostAsJsonAsync("/auth/register", credentials);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", credentials);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_NewUser_StoresOnlyHash()
    {
        // Act
        await _client.PostAsJsonAsync("/auth/register", ApiTestData.RegisterRequest());

        // Assert
        var user = _factory.Services.GetRequiredService<IUserRepository>().GetByUsername("owner");
        Assert.NotEqual(Examples.Password, user!.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
    }

    [Fact]
    public async Task PostAuthLogin_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var credentials = ApiTestData.RegisterRequest();
        await _client.PostAsJsonAsync("/auth/register", credentials);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", credentials);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body!.Token));
    }

    [Fact]
    public async Task PostAuthLogin_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        await _client.PostAsJsonAsync("/auth/register", ApiTestData.RegisterRequest());

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", new { Username = "owner", Password = "Wrong-password-1" });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAuthMe_WithToken_ReturnsUsername()
    {
        // Arrange
        var client = await AuthHelper.CreateLoggedInClientAsync(_factory, "owner");

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var claims = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>();
        Assert.Contains(claims!, c => c.Value == "owner");
    }

    [Fact]
    public async Task PostAuthRegister_InvalidRole_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", ApiTestData.RegisterRequest(role: "Janitor"));

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_MissingName_ReturnsBadRequest()
    {
        // Act: no firstName or lastName in the body
        var response = await _client.PostAsJsonAsync("/auth/register", new { Username = "owner", Password = Examples.Password, Role = Roles.FrontDesk });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_NewUser_StoresTheNames()
    {
        // Act
        await _client.PostAsJsonAsync("/auth/register", ApiTestData.RegisterRequest("kari"));

        // Assert
        var user = _factory.Services.GetRequiredService<IUserRepository>().GetByUsername("kari");
        Assert.Equal(Examples.EmployeeFirstName, user!.FirstName);
        Assert.Equal(Examples.EmployeeLastName, user.LastName);
    }

    [Fact]
    public async Task GetAuthMe_FrontDeskUser_ReturnsTheRole()
    {
        // Arrange
        var client = await AuthHelper.CreateLoggedInClientAsync(_factory, "frontdesk", Roles.FrontDesk);

        // Act
        var response = await client.GetAsync("/auth/me");

        // Assert
        var claims = await response.Content.ReadFromJsonAsync<List<ClaimResponse>>();
        Assert.Contains(claims!, c => c.Type.EndsWith("role") && c.Value == Roles.FrontDesk);
    }

    private record LoginResponse(string Token);
    private record ClaimResponse(string Type, string Value);
}
