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

    public static Task<HttpResponseMessage> RegisterPaymentAsync(this HttpClient client, Guid orderId, decimal amount = Examples.PaymentAmount) =>
        client.PostAsJsonAsync($"/orders/{orderId}/payments", NewPaymentRequest(amount));
}

// The answers as the API tests read them. Add a field when a test checks it, with the name from the response in api.md.
public record CustomerResponse(Guid Id, string Name, string Phone, string Email, string Address);
public record OrderResponse(Guid Id, Guid CustomerId, string WorkDescription, decimal InitialAgreedPrice, decimal OutstandingBalance, string Status, List<OrderHistoryResponse> History, List<PaymentResponse> Payments, bool IsDelivered);
public record OrderHistoryResponse(string? ActorDisplayName, string Type, string? Note);
public record PaymentResponse(Guid Id, decimal Amount, string TransactionReference);
