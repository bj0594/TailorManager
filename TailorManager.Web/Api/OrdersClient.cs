using System.Net.Http.Json;

namespace TailorManager.Web.Api;

// Calls the API over HTTP. The pages only know IOrdersClient.
public class OrdersClient : IOrdersClient
{
    private readonly HttpClient _http;

public OrdersClient(HttpClient http)
{
    _http = http;
}

// The test comes first, in the next step.
public Task<List<OrderDto>> GetOrdersAsync() => throw new NotImplementedException();
}
