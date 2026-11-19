using System.Net;
using System.Text.Json;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Tests AuthClient against answers the test gives it, so no API has to run.
public class AuthClientTests
{
    private readonly StubHttpMessageHandler _handler = new();
    private readonly TokenStore _tokens = new();
    private readonly AuthClient _auth;

    public AuthClientTests()
    {
        _auth = new AuthClient(_handler.CreateHttpClient(), _tokens);
    }

    [Fact]
    public async Task LoginAsync_RightPassword_StoresTheToken()
    {
        // Arrange
        _handler.Respond("/auth/login", """{ "token": "abc" }""").Respond("/auth/me", "[]");

        // Act
        var result = await _auth.LoginAsync("anna", "Secret-123");

        // Assert
        Assert.True(result);
        Assert.Equal("abc", _tokens.Token);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsFalse()
    {
        // Arrange
        _handler.Respond("/auth/login", "", HttpStatusCode.Unauthorized);

        // Act
        var result = await _auth.LoginAsync("anna", "wrong");

        // Assert
        Assert.False(result);
        Assert.Null(_tokens.Token);
    }

    [Fact]
    public async Task LoginAsync_WorkshopUser_StoresTheRole()
    {
        // Arrange
        _handler.Respond("/auth/login", """{ "token": "abc" }""")
                .Respond("/auth/me", """[{ "type": "role", "value": "Workshop" }]""");

        // Act
        await _auth.LoginAsync("anna", "Secret-123");

        // Assert
        Assert.Equal(Roles.Workshop, _tokens.Role);
    }

    [Fact]
    public async Task RegisterAsync_UsernameTaken_ReturnsTheMessage()
    {
        // Arrange
        _handler.Respond("/auth/register", "Username is already taken.", HttpStatusCode.Conflict);

        // Act
        var error = await _auth.RegisterAsync("anna", "Secret-123", "Anna", "Hansen", Roles.FrontDesk);

        // Assert
        Assert.Equal("Username is already taken.", error);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsNoError()
    {
        // Arrange
        _handler.Respond("/auth/register", "{}", HttpStatusCode.Created);

        // Act
        var error = await _auth.RegisterAsync("anna", "Secret-123", "Anna", "Hansen", Roles.FrontDesk);

        // Assert
        Assert.Null(error);
    }

    [Fact]
    public async Task RegisterAsync_MissingField_ReturnsAGeneralMessage()
    {
        // Arrange
        _handler.Respond("/auth/register", """{ "title": "One or more validation errors occurred." }""", HttpStatusCode.BadRequest);

        // Act
        var error = await _auth.RegisterAsync("anna", "Secret-123", "", "", Roles.FrontDesk);

        // Assert
        Assert.Equal("Fill in every field.", error);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_SendsTheRole()
    {
        // Arrange
        _handler.Respond("/auth/register", "{}", HttpStatusCode.Created);

        // Act
        await _auth.RegisterAsync("anna", "Secret-123", "Anna", "Hansen", Roles.Workshop);

        // Assert
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(Roles.Workshop, body.GetProperty("role").GetString());
    }

    [Fact]
    public async Task RegisterAsync_NewUser_SendsTheNames()
    {
        // Arrange
        _handler.Respond("/auth/register", "{}", HttpStatusCode.Created);

        // Act
        await _auth.RegisterAsync("anna", "Secret-123", "Anna", "Hansen", Roles.Workshop);

        // Assert
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal("Anna", body.GetProperty("firstName").GetString());
        Assert.Equal("Hansen", body.GetProperty("lastName").GetString());
    }
}
