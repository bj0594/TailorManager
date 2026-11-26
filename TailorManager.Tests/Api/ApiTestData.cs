using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using TailorManager.Core;

namespace TailorManager.Tests;

// What the API tests repeat: the request bodies with the example values from §4, logging in, and making a customer or an order.
//   var client = await _factory.LogInAsFrontDeskAsync();
//   var order = await client.CreateOrderAsync();
public static class ApiTestData
{
    // POST /auth/register (and /auth/login, which reads the username and the password from the same object).
    public static object RegisterRequest(string username = "owner", string role = Roles.FrontDesk, string firstName = Examples.EmployeeFirstName, string lastName = Examples.EmployeeLastName) =>
        new { username, password = Examples.Password, firstName, lastName, role };

    // POST /customers
    public static object NewCustomerRequest(string name = Examples.CustomerName) =>
        new { name, phone = Examples.CustomerPhone, email = Examples.CustomerEmail, address = Examples.CustomerAddress };

    // POST /orders/{orderId}/payments
    public static object NewPaymentRequest(decimal amount = Examples.PaymentAmount) =>
        new { amount, transactionReference = Examples.TransactionReference };

    // POST /orders
    public static object NewOrderRequest(Guid customerId, string workDescription = Examples.WorkDescription, decimal agreedPrice = Examples.AgreedPrice, Guid? relatedOrderId = null) =>
        new { customerId, type = "NewGarment", workDescription, agreedPrice, relatedOrderId };

    // Kari Nordmann at the front desk: she makes customers and orders.
    public static Task<HttpClient> LogInAsFrontDeskAsync(this WebApplicationFactory<Program> factory) =>
        AuthHelper.CreateLoggedInClientAsync(factory, "kari", Roles.FrontDesk);

    // Ola Nordmann in the workshop: he does the work on the orders.
    public static Task<HttpClient> LogInAsWorkshopAsync(this WebApplicationFactory<Program> factory) =>
        AuthHelper.CreateLoggedInClientAsync(factory, "ola", Roles.Workshop, "Ola", "Nordmann");

    // Kari or Ola, by the role a test needs.
    public static Task<HttpClient> LogInAsAsync(this WebApplicationFactory<Program> factory, string role) =>
        role == Roles.FrontDesk ? factory.LogInAsFrontDeskAsync() : factory.LogInAsWorkshopAsync();

    // POST /customers, and the customer from the answer. The client must be logged in as FrontDesk.
    public static async Task<CustomerResponse> CreateCustomerAsync(this HttpClient client, string name = Examples.CustomerName)
    {
        var response = await client.PostAsJsonAsync("/customers", NewCustomerRequest(name));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<CustomerResponse>())!;
    }

    // POST /orders, and the whole answer, for the tests that check the status code.
    public static Task<HttpResponseMessage> PostOrderAsync(this HttpClient client, Guid customerId, string workDescription = Examples.WorkDescription, decimal agreedPrice = Examples.AgreedPrice) =>
        client.PostAsJsonAsync("/orders", NewOrderRequest(customerId, workDescription, agreedPrice));

    // A new customer and an order for them, and the order from the answer. The client must be logged in as FrontDesk.
    public static async Task<OrderResponse> CreateOrderAsync(this HttpClient client, string workDescription = Examples.WorkDescription, decimal agreedPrice = Examples.AgreedPrice)
    {
        var customer = await client.CreateCustomerAsync();
        var response = await client.PostOrderAsync(customer.Id, workDescription, agreedPrice);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }

    // An order that is InProgress: Kari (frontDesk) makes it, and Ola (workshop) starts it.
    public static async Task<OrderResponse> CreateOrderInProgressAsync(this HttpClient frontDesk, HttpClient workshop)
    {
        var order = await frontDesk.CreateOrderAsync();
        (await workshop.StartWorkAsync(order.Id)).EnsureSuccessStatusCode();
        return order;
    }

    // An order that is ReadyForPickup: Kari (frontDesk) makes it, and Ola (workshop) starts it and marks it.
    public static async Task<OrderResponse> CreateOrderReadyForPickupAsync(this HttpClient frontDesk, HttpClient workshop, decimal agreedPrice = Examples.AgreedPrice)
    {
        var order = await frontDesk.CreateOrderAsync(agreedPrice: agreedPrice);
        (await workshop.StartWorkAsync(order.Id)).EnsureSuccessStatusCode();
        (await workshop.MarkReadyForPickupAsync(order.Id)).EnsureSuccessStatusCode();
        return order;
    }

    // The order actions in api.md. Each one gives the whole answer, so a test can check the status code.
    public static Task<HttpResponseMessage> StartWorkAsync(this HttpClient client, Guid orderId) =>
        client.PatchAsync($"/orders/{orderId}/start", null);

    public static Task<HttpResponseMessage> AddWorkNoteAsync(this HttpClient client, Guid orderId, string note = Examples.WorkNote) =>
        client.PostAsJsonAsync($"/orders/{orderId}/notes", new { note });

    public static Task<HttpResponseMessage> MarkReadyForPickupAsync(this HttpClient client, Guid orderId) =>
        client.PatchAsync($"/orders/{orderId}/ready-for-pickup", null);

    public static Task<HttpResponseMessage> MarkDeliveredAsync(this HttpClient client, Guid orderId) =>
        client.PatchAsync($"/orders/{orderId}/delivered", null);

    public static Task<HttpResponseMessage> RegisterPaymentAsync(this HttpClient client, Guid orderId, decimal amount = Examples.PaymentAmount) =>
        client.PostAsJsonAsync($"/orders/{orderId}/payments", NewPaymentRequest(amount));

    public static Task<HttpResponseMessage> RequestCustomerDecisionAsync(this HttpClient client, Guid orderId, decimal? proposedPriceAdjustment = Examples.ProposedPriceAdjustment) =>
        client.PostAsJsonAsync($"/orders/{orderId}/decision-requests", new { proposedWorkChange = Examples.ProposedWorkChange, proposedPriceAdjustment });

    public static Task<HttpResponseMessage> RecordCustomerDecisionAsync(this HttpClient client, Guid orderId, Guid requestId, bool approved = true) =>
        client.PostAsJsonAsync($"/orders/{orderId}/decision-requests/{requestId}/decision", new { communicationId = Guid.NewGuid(), approved });

    // An order that waits for the customer: Kari makes it, and Ola starts it and asks about the change. The pending request is the last in the list.
    public static async Task<OrderResponse> CreateOrderWaitingForCustomerAsync(this HttpClient frontDesk, HttpClient workshop, decimal? proposedPriceAdjustment = Examples.ProposedPriceAdjustment)
    {
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);
        return await (await workshop.RequestCustomerDecisionAsync(order.Id, proposedPriceAdjustment)).ReadOrderAsync();
    }

    // A fitting two days from now, so it is never in the past.
    public static Task<HttpResponseMessage> ScheduleFittingAsync(this HttpClient client, Guid orderId, DateTimeOffset? scheduledAt = null) =>
        client.PostAsJsonAsync($"/orders/{orderId}/fittings", new { scheduledAt = scheduledAt ?? DateTimeOffset.UtcNow.AddDays(2) });

    public static Task<HttpResponseMessage> CompleteFittingAsync(this HttpClient client, Guid orderId, Guid fittingId) =>
        client.PatchAsJsonAsync($"/orders/{orderId}/fittings/{fittingId}/complete", new { outcome = Examples.FittingOutcome });

    // An order that waits for a fitting: Kari makes it, and Ola starts it and schedules the fitting. The fitting is the last in the list.
    public static async Task<OrderResponse> CreateOrderWaitingForFittingAsync(this HttpClient frontDesk, HttpClient workshop)
    {
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);
        return await (await workshop.ScheduleFittingAsync(order.Id)).ReadOrderAsync();
    }

    // The communication in §4: an email from the customer. outcome: "Received", "Sent" or "Failed".
    public static Task<HttpResponseMessage> RecordCommunicationAsync(this HttpClient client, Guid orderId, string direction = "Incoming", string outcome = "Received") =>
        client.PostAsJsonAsync($"/orders/{orderId}/communications", new
        {
            channel = "Email",
            direction,
            source = direction == "Incoming" ? "Customer" : "Manual",
            content = Examples.CommunicationContent,
            outcome,
            occurredAt = DateTimeOffset.UtcNow
        });

    public static Task<HttpResponseMessage> RecordInitialApprovalAsync(this HttpClient client, Guid orderId, string? documentReference = Examples.ApprovalDocumentReference) =>
        client.PostAsJsonAsync($"/orders/{orderId}/approvals", new { method = "SignedForm", documentReference });

    public static Task<HttpResponseMessage> RecordInspectionFeeAgreementAsync(this HttpClient client, Guid orderId, decimal amount = Examples.InspectionFeeAmount, string? documentReference = Examples.InspectionFeeDocumentReference) =>
        client.PostAsJsonAsync($"/orders/{orderId}/inspection-fee-agreements", new { amount, documentReference });

    // The answer as an OrderResponse, after checking it is 200 OK: var body = await response.ReadOrderAsync();
    public static async Task<OrderResponse> ReadOrderAsync(this HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!;
    }
}

// The answers as the API tests read them. Add a field when a test checks it, with the name from the response in api.md.
public record CustomerResponse(Guid Id, string Name, string Phone, string Email, string Address);
public record OrderResponse(Guid Id, Guid CustomerId, string WorkDescription, decimal InitialAgreedPrice, decimal CurrentAgreedPrice, decimal OutstandingBalance, string Status, List<OrderHistoryResponse> History, List<PaymentResponse> Payments,
    List<CustomerDecisionRequestResponse> CustomerDecisionRequests, List<FittingResponse> Fittings, List<CommunicationResponse> Communications);
public record OrderHistoryResponse(string? ActorDisplayName, string Type, string? Note, DateTimeOffset OccurredAt);
public record PaymentResponse(Guid Id, decimal Amount, string TransactionReference);
public record CustomerDecisionRequestResponse(Guid Id, string ProposedWorkChange, decimal? ProposedPriceAdjustment, string Status);
public record FittingResponse(Guid Id, DateTimeOffset ScheduledAt, DateTimeOffset? CompletedAt, string? Outcome);
public record CommunicationResponse(Guid Id, string Channel, string Direction, string Source, string Content, string Outcome, DateTimeOffset OccurredAt);
public record ApprovalResponse(Guid Id, string Scope, string Method, string? DocumentReference, decimal? ApprovedAmount);
