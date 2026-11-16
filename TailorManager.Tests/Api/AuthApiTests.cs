using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Tests;

// Register, log in and /auth/me, through the API with the fake repositories.
public class AuthApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();
    private readonly HttpClient _client;

    public AuthApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostAuthRegister_NewUser_ReturnsCreated()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", new { Username = "employee", Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_TakenUsername_ReturnsConflict()
    {
        // Arrange
        var credentials = new { Username = "owner", Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk };
        await _client.PostAsJsonAsync("/auth/register", credentials);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", credentials);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_NewUser_StoresOnlyHash()
    {
        // Arrange
        var password = "Test-password-1";

        // Act
        await _client.PostAsJsonAsync("/auth/register", new { Username = "owner", Password = password, FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk });

        // Assert
        var user = _factory.Services.GetRequiredService<IUserRepository>().GetByUsername("owner");
        Assert.NotEqual(password, user!.PasswordHash);
        Assert.StartsWith("$2", user.PasswordHash);
    }

    [Fact]
    public async Task PostAuthLogin_ValidCredentials_ReturnsToken()
    {
        // Arrange
        var credentials = new { Username = "owner", Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk };
        await _client.PostAsJsonAsync("/auth/register", credentials);

        // Act
        var response = await _client.PostAsJsonAsync("/auth/login", credentials);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrEmpty(body!.Token));
    }

    private record LoginResponse(string Token);

    [Fact]
    public async Task PostAuthLogin_WrongPassword_ReturnsUnauthorized()
    {
        // Arrange
        await _client.PostAsJsonAsync("/auth/register", new { Username = "owner", Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = Roles.FrontDesk });

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
        var response = await _client.PostAsJsonAsync("/auth/register", new { Username = "owner", Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = "Janitor" });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAuthRegister_MissingName_ReturnsBadRequest()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/auth/register", new { Username = "owner", Password = "Test-password-1", Role = Roles.FrontDesk });

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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

    private record ClaimResponse(string Type, string Value);
}
