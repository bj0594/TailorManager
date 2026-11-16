using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know ICustomersClient.
public class CustomersClient : ICustomersClient
{
    private readonly HttpClient _http;

    public CustomersClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<CustomerDto>> GetCustomersAsync()
    {
        return await _http.GetFromJsonAsync<List<CustomerDto>>("customers") ?? [];
    }
}
