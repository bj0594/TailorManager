using System.Text.Json;

namespace TailorManager.Tests;

// Swagger shows the Authorize button when the OpenAPI document has the Bearer scheme.
public class OpenApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task OpenApiDocument_Always_HasTheBearerScheme()
    {
        // Act
        var json = await _factory.CreateClient().GetStringAsync("/openapi/v1.json");

        // Assert
        var schemes = JsonDocument.Parse(json).RootElement.GetProperty("components").GetProperty("securitySchemes");
        Assert.True(schemes.TryGetProperty("Bearer", out _));
    }
}
