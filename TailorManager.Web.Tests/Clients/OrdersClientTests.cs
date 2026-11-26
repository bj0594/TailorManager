using System.Net;
using System.Text.Json;
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
    public async Task ListOrdersAsync_ApiReturnsTwo_ReturnsBoth()
    {
        // Arrange
        _handler.Respond("/orders", TestDtos.ToJson(new[] { TestDtos.NewOrder(), TestDtos.NewOrder(workDescription: Examples.OtherWorkDescription) }));

        // Act
        var result = await _client.ListOrdersAsync();

        // Assert
        Assert.Equal(2, result.Count);
        _handler.AssertSent(HttpMethod.Get, "/orders");
    }

    [Fact]
    public async Task ListOrdersAsync_LoggedIn_SendsTheToken()
    {
        // Arrange
        var client = new OrdersClient(_handler.CreateHttpClient(), new TokenStore { Token = "abc" });
        _handler.Respond("/orders", "[]");

        // Act
        await client.ListOrdersAsync();

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
        _handler.AssertSent(HttpMethod.Get, $"/orders/{order.Id}");
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
        _handler.AssertSent(HttpMethod.Patch, $"/orders/{id}/start");
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
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/notes");
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
        _handler.AssertSent(HttpMethod.Patch, $"/orders/{id}/ready-for-pickup");
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
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/payments");
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
        _handler.AssertSent(HttpMethod.Patch, $"/orders/{id}/delivered");
    }

    [Fact]
    public async Task ListOrdersAsync_WithStatusAndCustomer_SendsThemInTheQuery()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _handler.Respond("/orders", "[]");

        // Act
        await _client.ListOrdersAsync("InProgress", customerId);

        // Assert
        Assert.Equal($"?status=InProgress&customerId={customerId}", _handler.Requests[0].RequestUri!.Query);
    }

    [Fact]
    public async Task CreateOrderAsync_Valid_PostsTheOrder()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        _handler.Respond("/orders", TestDtos.ToJson(TestDtos.NewOrder(customerId)), HttpStatusCode.Created);

        // Act
        var result = await _client.CreateOrderAsync(customerId, "NewGarment", Examples.WorkDescription, Examples.AgreedPrice);

        // Assert
        _handler.AssertSent(HttpMethod.Post, "/orders");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(customerId, body.GetProperty("customerId").GetGuid());
        Assert.Equal(Examples.WorkDescription, body.GetProperty("workDescription").GetString());
        Assert.Equal(Examples.AgreedPrice, body.GetProperty("agreedPrice").GetDecimal());
        Assert.Equal(customerId, result.CustomerId);
    }

    [Fact]
    public async Task RequestCustomerDecisionAsync_Always_PostsTheRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/decision-requests", "{}");

        // Act
        await _client.RequestCustomerDecisionAsync(id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment);

        // Assert
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/decision-requests");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(Examples.ProposedWorkChange, body.GetProperty("proposedWorkChange").GetString());
        Assert.Equal(Examples.ProposedPriceAdjustment, body.GetProperty("proposedPriceAdjustment").GetDecimal());
    }

    [Fact]
    public async Task RecordCustomerDecisionAsync_Always_PostsTheDecision()
    {
        // Arrange
        var id = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        var communicationId = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/decision-requests/{requestId}/decision", "{}");

        // Act
        await _client.RecordCustomerDecisionAsync(id, requestId, communicationId, true);

        // Assert
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/decision-requests/{requestId}/decision");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(communicationId, body.GetProperty("communicationId").GetGuid());
        Assert.True(body.GetProperty("approved").GetBoolean());
    }

    [Fact]
    public async Task ScheduleFittingAsync_Always_PostsTheDate()
    {
        // Arrange
        var id = Guid.NewGuid();
        var scheduledAt = new DateTimeOffset(2026, 11, 26, 10, 0, 0, TimeSpan.FromHours(1));
        _handler.Respond($"/orders/{id}/fittings", "{}");

        // Act
        await _client.ScheduleFittingAsync(id, scheduledAt);

        // Assert
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/fittings");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(scheduledAt, body.GetProperty("scheduledAt").GetDateTimeOffset());
    }

    [Fact]
    public async Task CompleteFittingAsync_Always_PatchesTheOutcome()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fittingId = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/fittings/{fittingId}/complete", "{}");

        // Act
        await _client.CompleteFittingAsync(id, fittingId, Examples.FittingOutcome);

        // Assert
        _handler.AssertSent(HttpMethod.Patch, $"/orders/{id}/fittings/{fittingId}/complete");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal(Examples.FittingOutcome, body.GetProperty("outcome").GetString());
    }

    [Fact]
    public async Task RecordCommunicationAsync_Always_PostsTheMessage()
    {
        // Arrange
        var id = Guid.NewGuid();
        var communication = new CommunicationInput("Email", "Incoming", "Customer", Examples.CommunicationContent, "Received", DateTimeOffset.Now);
        _handler.Respond($"/orders/{id}/communications", "{}");

        // Act
        await _client.RecordCommunicationAsync(id, communication);

        // Assert
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/communications");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal("Incoming", body.GetProperty("direction").GetString());
        Assert.Equal(Examples.CommunicationContent, body.GetProperty("content").GetString());
    }

    [Fact]
    public async Task RecordInitialApprovalAsync_Always_PostsTheSignedForm()
    {
        // Arrange
        var id = Guid.NewGuid();
        _handler.Respond($"/orders/{id}/approvals", "{}", HttpStatusCode.Created);

        // Act
        await _client.RecordInitialApprovalAsync(id, "SignedForm", Examples.ApprovalDocumentReference);

        // Assert
        _handler.AssertSent(HttpMethod.Post, $"/orders/{id}/approvals");
        var body = JsonDocument.Parse(_handler.Bodies.Single()!).RootElement;
        Assert.Equal("SignedForm", body.GetProperty("method").GetString());
        Assert.Equal(Examples.ApprovalDocumentReference, body.GetProperty("documentReference").GetString());
    }
}
