using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know IOrdersClient.
public class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;

    public OrdersClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        return await _http.GetFromJsonAsync<List<OrderDto>>("orders") ?? [];
    }
}
