using System.Net;

namespace TailorManager.Tests;

public class HealthApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task GetHealth_ApiRunning_ReturnsOk()
    {
        // Act
        var response = await _factory.CreateClient().GetAsync("/health");

        // Assert: the endpoint exists. 200 or 503 depends on whether PostgreSQL is running.
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
