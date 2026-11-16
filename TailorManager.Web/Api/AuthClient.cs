using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Logs in through the API and keeps the token in TokenStore.
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
        return true;
    }

    public async Task<bool> RegisterAsync(string username, string password, string role)
    {
        var response = await _http.PostAsJsonAsync("auth/register", new { username, password, role });
        return response.IsSuccessStatusCode;
    }

    private record LoginResponse(string Token);
}
