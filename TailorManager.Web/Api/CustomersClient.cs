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

    public async Task<List<CustomerDto>> FindCustomersAsync(string? searchTerm = null)
    {
        var url = searchTerm is null ? "customers" : $"customers?searchTerm={Uri.EscapeDataString(searchTerm)}";
        return await _http.GetFromJsonAsync<List<CustomerDto>>(url) ?? [];
    }

    public async Task<CustomerDto> AddCustomerAsync(string name, string phone, string email, string address)
    {
        var response = await _http.PostAsJsonAsync("customers", new { name, phone, email, address }).EnsureSuccessAsync();
        return (await response.Content.ReadFromJsonAsync<CustomerDto>())!;
    }

    public async Task<CustomerDto> GetCustomerAsync(Guid id) =>
        (await _http.GetFromJsonAsync<CustomerDto>($"customers/{id}"))!;
}
