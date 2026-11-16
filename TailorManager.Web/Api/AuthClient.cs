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

    // The test comes first, below. GREEN in point 3 replaces this file.
    public Task<bool> LoginAsync(string username, string password) => throw new NotImplementedException();
}
