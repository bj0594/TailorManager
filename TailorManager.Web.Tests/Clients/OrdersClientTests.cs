using System.Net;
using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Tests the real client against answers the test gives it, so no API has to run.
public class OrdersClientTests
{
    private readonly StubHttpMessageHandler _handler = new();
    private readonly OrdersClient _client;

    public OrdersClientTests()
    {
        _client = new OrdersClient(_handler.CreateHttpClient(), new TokenStore());
    }

    [Fact]
    public async Task GetOrdersAsync_ApiReturnsTwo_ReturnsBoth()
    {
        // Arrange
        _handler.Respond("/orders", TestDtos.ToJson(new[] { TestDtos.NewOrder(), TestDtos.NewOrder(workDescription: Examples.OtherWorkDescription) }));

        // Act
        var result = await _client.GetOrdersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal("/orders", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task GetOrdersAsync_LoggedIn_SendsTheToken()
    {
        // Arrange
        var client = new OrdersClient(_handler.CreateHttpClient(), new TokenStore { Token = "abc" });
        _handler.Respond("/orders", "[]");

        // Act
        await client.GetOrdersAsync();

        // Assert
        Assert.Equal("Bearer abc", _handler.Requests.Single().Headers.Authorization?.ToString());
    }

    [Fact]
    public async Task GetOrderAsync_ExistingId_ReadsIt()
    {
        // Arrange
        var order = TestDtos.NewOrder();
        _handler.Respond($"/orders/{order.Id}", TestDtos.ToJson(order));

        // Act
        var result = await _client.GetOrderAsync(order.Id);

        // Assert
        Assert.Equal(HttpMethod.Get, _handler.Requests.Single().Method);
        Assert.Equal(Examples.WorkDescription, result.WorkDescription);
    }

    [Fact]
    public async Task StartWorkAsync_Always_SendsPatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/start", "", HttpStatusCode.NoContent);

        // Act
        await _client.StartWorkAsync(id);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"/orders/{id}/start", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task StartWorkAsync_ApiAnswers409_ThrowsWithTheMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/start", "The order is not Submitted.", HttpStatusCode.Conflict);

        // Act
        var ex = await Assert.ThrowsAsync<HttpRequestException>(() => _client.StartWorkAsync(id));

        // Assert
        Assert.Equal("The order is not Submitted.", ex.Message);
    }

    [Fact]
    public async Task AddWorkNoteAsync_Always_PostsTheWorkNote()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/notes", "{}");

        // Act
        await _client.AddWorkNoteAsync(id, Examples.WorkNote);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"/orders/{id}/notes", request.RequestUri!.AbsolutePath);
        Assert.Contains(Examples.WorkNote, _handler.Bodies.Single());
    }

    [Fact]
    public async Task MarkReadyForPickupAsync_Always_SendsPatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/ready-for-pickup", "{}");

        // Act
        await _client.MarkReadyForPickupAsync(id);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"/orders/{id}/ready-for-pickup", request.RequestUri!.AbsolutePath);
    }

    [Fact]
    public async Task RegisterPaymentAsync_Always_PostsThePayment()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/payments", "{}");

        // Act
        await _client.RegisterPaymentAsync(id, Examples.PaymentAmount, Examples.TransactionReference);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"/orders/{id}/payments", request.RequestUri!.AbsolutePath);
        var body = _handler.Bodies.Single();
        Assert.Contains(Examples.PaymentAmount.ToString(), body);
        Assert.Contains(Examples.TransactionReference, body);
    }

    [Fact]
    public async Task MarkDeliveredAsync_Always_SendsPatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/delivered", "{}");

        // Act
        await _client.MarkDeliveredAsync(id);

        // Assert
        var request = _handler.Requests.Single();
        Assert.Equal(HttpMethod.Patch, request.Method);
        Assert.Equal($"/orders/{id}/delivered", request.RequestUri!.AbsolutePath);
    }
}
