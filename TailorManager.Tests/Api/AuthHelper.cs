using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TailorManager.Core;

namespace TailorManager.Tests;

// Registers and logs in a user through the API, and gives a client that sends the token with every request.
// The shortcuts _factory.LogInAsFrontDeskAsync() and _factory.LogInAsWorkshopAsync() are in ApiTestData.
public static class AuthHelper
{
    public static async Task<HttpClient> CreateLoggedInClientAsync(WebApplicationFactory<Program> factory, string username, string role = Roles.Workshop, string firstName = Examples.EmployeeFirstName, string lastName = Examples.EmployeeLastName)
    {
        var client = factory.CreateClient();
        var credentials = ApiTestData.RegisterRequest(username, role, firstName, lastName);

        await client.PostAsJsonAsync("/auth/register", credentials);
        var response = await client.PostAsJsonAsync("/auth/login", credentials);
        var login = await response.Content.ReadFromJsonAsync<LoginResponse>();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    private record LoginResponse(string Token);
}
