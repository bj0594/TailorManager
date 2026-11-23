using System.Net;
using System.Net.Http.Json;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class OrdersApiTests : IDisposable
{
    private readonly TestApiFactory _factory = new();
    private readonly HttpClient _client;

    public OrdersApiTests()
    {
        _client = _factory.CreateClient();
    }

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task PostOrders_ValidOrder_ReturnsCreated()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.PostOrderAsync(customer.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(customer.Id, body!.CustomerId);
    }

    [Fact]
    public async Task PostOrders_FreeOrder_ReturnsZeroOutstandingBalance()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.PostAsJsonAsync(
            "/orders",
            new
            {
                customerId = customer.Id,
                type = "NewGarment",
                workDescription = Examples.WorkDescription,
                agreedPrice = 0m,
                relatedOrderId = (Guid?)null
            });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();

        Assert.NotNull(body);
        Assert.Equal(0m, body.OutstandingBalance);
    }

    [Fact]
    public async Task PostOrders_WithoutToken_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/orders", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostOrders_NotFrontDesk_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var user = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await user.PostAsJsonAsync("/orders", new { });

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostOrders_LoggedIn_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var customer = await client.CreateCustomerAsync();

        // Act
        var response = await client.PostOrderAsync(customer.Id);

        // Assert
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(Examples.EmployeeDisplayName, body!.History[0].ActorDisplayName);
    }

    [Fact]
    public async Task GetOrder_ExistingId_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();

        // Act
        var response = await client.GetAsync($"/orders/{order.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(order.Id, body!.Id);
    }

    [Fact]
    public async Task PatchOrdersStart_ExistingOrder_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await workshop.StartWorkAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("InProgress", body!.Status);
        Assert.Contains(body.History, h => h.Type == "StatusChanged" && h.ActorDisplayName == "Ola N.");
    }

    [Fact]
    public async Task PatchOrdersStart_NotWorkshop_ReturnsForbidden()
    {
        // Arrange: a logged-in user without the role
        var client = await _factory.LogInAsFrontDeskAsync();

        // Act
        var response = await client.StartWorkAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PatchOrdersStart_NotSubmitted_ReturnsConflict()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var order = await client.CreateOrderAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        await workshop.StartWorkAsync(order.Id);

        // Act
        var response = await workshop.StartWorkAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("The order is not Submitted.", (await response.Content.ReadAsStringAsync()).Trim('"'));
    }

    [Fact]
    public async Task PostOrdersNotes_OrderInProgress_ReturnsOk()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await client.CreateOrderAsync();
        await workshop.StartWorkAsync(order.Id);

        // Act
        var response = await workshop.AddWorkNoteAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Contains(body!.History, h => h.Type == "WorkNoteAdded" && h.Note == Examples.WorkNote);
    }

    [Fact]
    public async Task PostOrdersNotes_NotWorkshop_ReturnsForbidden()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();

        // Act
        var response = await frontDesk.AddWorkNoteAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PatchOrdersReadyForPickup_OrderInProgress_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var order = await frontDesk.CreateOrderAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        await workshop.StartWorkAsync(order.Id);

        // Act
        var response = await workshop.MarkReadyForPickupAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("ReadyForPickup", body!.Status);
        Assert.Contains(body.History, h => h.Type == "NotificationRequested");
    }

    [Fact]
    public async Task PatchOrdersReadyForPickup_NotWorkshop_ReturnsForbidden()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();

        // Act
        var response = await frontDesk.MarkReadyForPickupAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PostOrdersPayments_FullPayment_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderReadyForPickupAsync(workshop);

        // Act
        var response = await frontDesk.RegisterPaymentAsync(order.Id, Examples.PaymentAmount);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(0m, body!.OutstandingBalance);
        Assert.Equal("Delivered", body.Status);
        Assert.Equal(Examples.TransactionReference, Assert.Single(body.Payments).TransactionReference);
    }

    [Fact]
    public async Task PostOrdersPayment_NotFrontDesk_ReturnsForbidden()
    {
        // Arrange
        var workshop = await _factory.LogInAsWorkshopAsync();

        // Act
        var response = await workshop.RegisterPaymentAsync(Guid.NewGuid());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task PatchOrdersDelivered_FreeOrderReadyForPickup_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderReadyForPickupAsync(workshop, agreedPrice: 0m);

        // Act
        var response = await frontDesk.MarkDeliveredAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal("Delivered", body!.Status);
    }
}
