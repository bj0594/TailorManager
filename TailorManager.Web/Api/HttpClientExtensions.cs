using System.Net.Http.Headers;

namespace TailorManager.Web.Api;

// What every client does with its HttpClient.
public static class HttpClientExtensions
{
    // Sends the logged-in user's token with every request from this HttpClient: _http.UseToken(tokens);
    public static void UseToken(this HttpClient http, TokenStore tokens)
    {
        if (tokens.Token is not null)
        {
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        }
    }

    // Waits for the answer, and throws with the API's own message when it says no (like 409), so the page can show it:
    //   _http.PatchAsync($"orders/{id}/start", null).EnsureSuccessAsync();
    public static async Task<HttpResponseMessage> EnsureSuccessAsync(this Task<HttpResponseMessage> sending)
    {
        var response = await sending;
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(message.Trim('"'), null, response.StatusCode);
        }
        return response;
    }
}
