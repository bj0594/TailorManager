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

// The test comes first, in the next step.
public Task<List<CustomerDto>> GetCustomersAsync() => throw new NotImplementedException();
}
