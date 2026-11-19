using System.Net.Http.Headers;

namespace TailorManager.Web.Api;

// Sends the logged-in user's token with every request from this HttpClient. Every client calls it: _http.UseToken(tokens);
public static class HttpClientExtensions
{
    public static void UseToken(this HttpClient http, TokenStore tokens)
    {
        if (tokens.Token is not null)
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        }
    }
}
