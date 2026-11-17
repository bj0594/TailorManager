using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know ICustomersClient.
public class CustomersClient : ICustomersClient
{
    private readonly HttpClient _http;

    public CustomersClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        if (tokens.Token is not null)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        }
    }

    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        return await _http.GetFromJsonAsync<List<CustomerDto>>("customers") ?? [];
    }

    public async Task AddCustomerAsync(string name, string phone, string email, string address)
    {
        var response = await _http.PostAsJsonAsync("customers", new { name, phone, email, address });
        response.EnsureSuccessStatusCode();
    }
}
