namespace TailorManager.Tests;

// The base class for the API tests: a new API in memory for every test (TestApiFactory), and _client without a token.
// A logged-in client comes from _factory.LogInAsFrontDeskAsync() or _factory.LogInAsWorkshopAsync() (ApiTestData).
public abstract class ApiTestBase : IDisposable
{
    protected readonly TestApiFactory _factory = new();
    protected readonly HttpClient _client;

    protected ApiTestBase() => _client = _factory.CreateClient();

    public void Dispose() => _factory.Dispose();
}
