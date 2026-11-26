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

    public async Task<OrderDto> CreateOrderAsync(Guid customerId, string type, string workDescription, decimal agreedPrice)
    {
        var response = await _http.PostAsJsonAsync("orders", new { customerId, type, workDescription, agreedPrice }).EnsureSuccessAsync();
        return (await response.Content.ReadFromJsonAsync<OrderDto>())!;
    }

    public Task StartWorkAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/start", null).EnsureSuccessAsync();

    public Task AddWorkNoteAsync(Guid id, string note) =>
        _http.PostAsJsonAsync($"orders/{id}/notes", new { note }).EnsureSuccessAsync();

    public Task MarkReadyForPickupAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/ready-for-pickup", null).EnsureSuccessAsync();

    public Task RequestCustomerDecisionAsync(Guid id, string proposedWorkChange, decimal? proposedPriceAdjustment) =>
        _http.PostAsJsonAsync($"orders/{id}/decision-requests", new { proposedWorkChange, proposedPriceAdjustment }).EnsureSuccessAsync();

    public Task RecordCustomerDecisionAsync(Guid id, Guid requestId, Guid communicationId, bool approved) =>
        _http.PostAsJsonAsync($"orders/{id}/decision-requests/{requestId}/decision", new { communicationId, approved }).EnsureSuccessAsync();

    public Task ScheduleFittingAsync(Guid id, DateTimeOffset scheduledAt) =>
        _http.PostAsJsonAsync($"orders/{id}/fittings", new { scheduledAt }).EnsureSuccessAsync();

    public Task CompleteFittingAsync(Guid id, Guid fittingId, string outcome) =>
        _http.PatchAsJsonAsync($"orders/{id}/fittings/{fittingId}/complete", new { outcome }).EnsureSuccessAsync();

    // PostAsJsonAsync writes the fields in camelCase, like "direction" in api.md.
    public Task RecordCommunicationAsync(Guid id, CommunicationInput communication) =>
        _http.PostAsJsonAsync($"orders/{id}/communications", communication).EnsureSuccessAsync();

    public Task RecordInitialApprovalAsync(Guid id, string method, string? documentReference) =>
        _http.PostAsJsonAsync($"orders/{id}/approvals", new { method, documentReference }).EnsureSuccessAsync();

    public Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference) =>
        _http.PostAsJsonAsync($"orders/{id}/payments", new { amount, transactionReference }).EnsureSuccessAsync();

    public Task MarkDeliveredAsync(Guid id) =>
        _http.PatchAsync($"orders/{id}/delivered", null).EnsureSuccessAsync();

    public async Task<List<OrderDto>> ListOrdersAsync(string? status = null, Guid? customerId = null)
    {
        var filters = new List<string>();
        if (status is not null) filters.Add($"status={status}");
        if (customerId is not null) filters.Add($"customerId={customerId}");
        var url = filters.Count == 0 ? "orders" : $"orders?{string.Join("&", filters)}";
        return await _http.GetFromJsonAsync<List<OrderDto>>(url) ?? [];
    }

    public async Task<List<OrderDto>> GetOrderAsync(string? status = null, Guid? customerId = null)
    {
        var url = status is null ? "orders" : $"order?status={ status }";
        return await _http.GetFromJsonAsync<List<OrderDto>>(url) ?? [];
    }
}
