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

    public Task StartWorkAsync(Guid id) => SendAsync(_http.PatchAsync($"orders/{id}/start", null));

    public Task AddWorkNoteAsync(Guid id, string note) => SendAsync(_http.PostAsJsonAsync($"orders/{id}/notes", new { note }));

    public Task MarkReadyForPickupAsync(Guid id) => SendAsync(_http.PatchAsync($"orders/{id}/ready-for-pickup", null));

    public Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference) => SendAsync(_http.PostAsJsonAsync($"orders/{id}/payments", new { amount, transactionReference }));

    public Task MarkDeliveredAsync(Guid id) => SendAsync(_http.PatchAsync($"orders/{id}/delivered", null));

    // Waits for the answer, and throws with the API's own message when it says no, so the page can show it.
    private static async Task SendAsync(Task<HttpResponseMessage> sending)
    {
        var response = await sending;
        if (!response.IsSuccessStatusCode)
        {
            var message = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(message.Trim('"'), null, response.StatusCode);
        }
    }
}
