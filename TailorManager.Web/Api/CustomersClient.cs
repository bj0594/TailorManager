using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know ICustomersClient.
public class CustomersClient : ICustomersClient
{
    private readonly HttpClient _http;

    public CustomersClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        _http.UseToken(tokens);
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

    public async Task<CustomerDto> GetCustomerAsync(Guid id)
    {
        var customer = await _http.GetFromJsonAsync<CustomerDto>($"customers/{id}");
        return customer!;
    }
}
