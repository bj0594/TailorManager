using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know IOrdersClient.
public class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;

    public OrdersClient(HttpClient http, TokenStore tokens)
    {
        _http = http;
        _http.UseToken(tokens);
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

    public async Task StartOrderAsync(Guid id)
    {
        var response = await _http.PatchAsync($"orders/{id}/start", null);
        response.EnsureSuccessStatusCode();
    }
}
