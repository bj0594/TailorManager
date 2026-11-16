using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TailorManager.Core;

namespace TailorManager.Tests;

// Registers and logs in a user through the API, and gives a client that sends the token with every request.
public static class AuthHelper
{
    public static async Task<HttpClient> CreateLoggedInClientAsync(WebApplicationFactory<Program> factory, string username, string role = Roles.Workshop)
    {
        var client = factory.CreateClient();
        var credentials = new { Username = username, Password = "Test-password-1", FirstName = "Kari", LastName = "Nordmann", Role = role };

        await client.PostAsJsonAsync("/auth/register", credentials);
        var response = await client.PostAsJsonAsync("/auth/login", credentials);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    private record LoginResponse(string Token);
}
