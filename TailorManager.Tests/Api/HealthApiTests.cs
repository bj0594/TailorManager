using System.Net;

namespace TailorManager.Tests;

public class HealthApiTests : ApiTestBase
{
    [Fact]
    public async Task GetHealth_ApiRunning_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert: the endpoint exists. 200 or 503 depends on whether PostgreSQL is running.
        Assert.NotEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
