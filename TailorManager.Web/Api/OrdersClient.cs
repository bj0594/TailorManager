using System.Net.Http.Json;
using System.Net.Http.Headers;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know IOrdersClient.
public class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;

    public OrdersClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        if (tokens.Token is not null)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.Token);
        }
    }

    public async Task<List<OrderDto>> GetOrdersAsync()
    {
        return await _http.GetFromJsonAsync<List<OrderDto>>("orders") ?? [];
    }

    public Task AddOrderAsync(
        Guid Id, 
        Guid CustomerId, 
        string Type, 
        string WorkDescription, 
        decimal InitialAgreedPrice, 
        decimal CurrentAgreedPrice, 
        decimal OutstandingBalance, 
        string Status, 
        DateTimeOffset CreatedAt, 
        Guid? RelatedOrderId
    ) => throw new NotImplementedException();

    public async Task<OrderDto> GetOrderAsync(Guid id)
    {
        var Order= await _http.GetFromJsonAsync<OrderDto>($"orders/{id}");
        return Order!;
    }
}
