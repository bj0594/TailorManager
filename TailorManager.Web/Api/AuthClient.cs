using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Logs in and registers through the API, and keeps the token and the role in TokenStore.
public class AuthClient : IAuthClient
{
    private readonly HttpClient _http;
    private readonly TokenStore _tokens;

    public AuthClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        _tokens = tokens;
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        var response = await _http.PostAsJsonAsync("auth/login", new { username, password });
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        _tokens.Token = body!.Token;

        // Asks the API who is logged in, and keeps the role.
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _tokens.Token);
        var claims = await _http.GetFromJsonAsync<List<ClaimResponse>>("auth/me");
        _tokens.Role = claims?.FirstOrDefault(c => c.Type == "role" || c.Type.EndsWith("/role"))?.Value;
        return true;
    }

    public async Task<string?> RegisterAsync(string username, string password, string firstName, string lastName, string role)
    {
        var response = await _http.PostAsJsonAsync("auth/register", new { username, password, firstName, lastName, role });
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        // 409 and the role check answer with the message as text. A missing field answers with a validation object ({ … }) instead.
        var text = await response.Content.ReadAsStringAsync();
        return text.StartsWith('{') ? "Fill in every field." : text.Trim('"');
    }

    private record LoginResponse(string Token);
    private record ClaimResponse(string Type, string Value);
}
