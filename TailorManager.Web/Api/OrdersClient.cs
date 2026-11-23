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

    public async Task<List<OrderDto>> GetOrdersAsync() =>
        await _http.GetFromJsonAsync<List<OrderDto>>("orders") ?? [];

    public async Task<OrderDto> GetOrderAsync(Guid id) =>
        (await _http.GetFromJsonAsync<OrderDto>($"orders/{id}"))!;

    public Task StartWorkAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/start", null).EnsureSuccessAsync();

    public Task AddWorkNoteAsync(Guid id, string note) =>
        _http.PostAsJsonAsync($"orders/{id}/notes", new { note }).EnsureSuccessAsync();

    public Task MarkReadyForPickupAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/ready-for-pickup", null).EnsureSuccessAsync();

    public Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference) =>
        _http.PostAsJsonAsync($"orders/{id}/payments", new { amount, transactionReference }).EnsureSuccessAsync();

    public Task MarkDeliveredAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/delivered", null).EnsureSuccessAsync();
}
