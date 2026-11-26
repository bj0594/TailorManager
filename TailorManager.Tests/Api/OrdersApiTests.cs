using System.Net;
using System.Net.Http.Json;
using TailorManager.Core;

namespace TailorManager.Tests;

// Starts the API in memory with the fake repository, so the tests need no database.
public class OrdersApiTests : ApiTestBase
{
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

    // Each order action is for one role only (§4). The other role gets 403 before the order is looked up,
    // so a random id is enough. A new 403 cycle is one more line here (RED), and the [Authorize] on the action (GREEN).
    [Theory]
    [InlineData(Roles.FrontDesk, "PATCH", "start")]
    [InlineData(Roles.FrontDesk, "POST", "notes")]
    [InlineData(Roles.FrontDesk, "PATCH", "ready-for-pickup")]
    [InlineData(Roles.FrontDesk, "POST", "decision-requests")]
    [InlineData(Roles.FrontDesk, "POST", "decision-requests/5f8d2c1e-3a4b-4c6d-8e9f-0a1b2c3d4e5f/decision")]
    [InlineData(Roles.FrontDesk, "POST", "fittings")]
    [InlineData(Roles.FrontDesk, "PATCH", "fittings/5f8d2c1e-3a4b-4c6d-8e9f-0a1b2c3d4e5f/complete")]
    [InlineData(Roles.FrontDesk, "POST", "communications")]
    [InlineData(Roles.Workshop, "POST", "approvals")]
    [InlineData(Roles.Workshop, "POST", "inspection-fee-agreements")]
    [InlineData(Roles.Workshop, "POST", "payments")]
    [InlineData(Roles.Workshop, "PATCH", "delivered")]
    public async Task OrderActions_OtherRole_ReturnsForbidden(string role, string method, string action)
    {
        // Arrange
        var client = await _factory.LogInAsAsync(role);
        var request = new HttpRequestMessage(new HttpMethod(method), $"/orders/{Guid.NewGuid()}/{action}") { Content = JsonContent.Create(new { }) };

        // Act
        var response = await client.SendAsync(request);

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

    [Fact]
    public async Task GetOrders_WithStatusAndCustomer_ReturnsOnlyMatching()
    {
        // Arrange
        var client = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var customer = await client.CreateCustomerAsync();
        var match = await (await client.PostOrderAsync(customer.Id)).Content.ReadFromJsonAsync<OrderResponse>();
        await workshop.StartWorkAsync(match!.Id);
        await client.PostOrderAsync(customer.Id);         
        var other = await client.CreateOrderAsync();       
        await workshop.StartWorkAsync(other.Id);           

        // Act
        var response = await client.GetAsync($"/orders?status=InProgress&customerId={customer.Id}");

        // Assert
        var body = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        Assert.Equal(match.Id, Assert.Single(body!).Id);
    }

    [Fact]
    public async Task GetOrder_AfterStart_ReturnsTheHistoryOldestFirst()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderAsync();
        (await workshop.StartWorkAsync(order.Id)).EnsureSuccessStatusCode();

        // Act
        var response = await workshop.GetAsync($"/orders/{order.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(new[] { "OrderCreated", "StatusChanged" }, body!.History.Select(h => h.Type));
        Assert.NotEqual(default, body.History[0].OccurredAt);
        Assert.True(body.History[0].OccurredAt <= body.History[1].OccurredAt);
    }

    [Fact]
    public async Task PostOrdersDecisionRequests_OrderInProgress_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);

        // Act
        var response = await workshop.RequestCustomerDecisionAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("WaitingForCustomer", body.Status);
        var request = Assert.Single(body.CustomerDecisionRequests);
        Assert.Equal("Pending", request.Status);
        Assert.Equal(Examples.ProposedPriceAdjustment, request.ProposedPriceAdjustment);
    }

    [Fact]
    public async Task PostOrdersDecisionRequests_AlreadyWaiting_ReturnsOkStillWaiting()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);
        (await workshop.RequestCustomerDecisionAsync(order.Id)).EnsureSuccessStatusCode();

        // Act
        var response = await workshop.RequestCustomerDecisionAsync(order.Id, null);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("WaitingForCustomer", body.Status);
        Assert.Equal(2, body.CustomerDecisionRequests.Count);
    }

    [Fact]
    public async Task PostOrdersDecision_Approved_ReturnsOkWithTheNewPrice()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop);
        var request = order.CustomerDecisionRequests.Single();

        // Act
        var response = await workshop.RecordCustomerDecisionAsync(order.Id, request.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal(Examples.AgreedPrice + Examples.ProposedPriceAdjustment, body.CurrentAgreedPrice);
        Assert.Equal("Approved", body.CustomerDecisionRequests.Single().Status);
        Assert.Equal("InProgress", body.Status);
    }

    [Fact]
    public async Task PostOrdersDecision_ApprovedWithoutPrice_ReturnsOkWithThePriceUnchanged()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop, proposedPriceAdjustment: null);

        // Act
        var response = await workshop.RecordCustomerDecisionAsync(order.Id, order.CustomerDecisionRequests.Single().Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("Approved", body.CustomerDecisionRequests.Single().Status);
        Assert.Equal(Examples.AgreedPrice, body.CurrentAgreedPrice);
    }

    [Fact]
    public async Task PostOrdersDecision_Rejected_ReturnsOkWithThePriceUnchanged()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop);

        // Act
        var response = await workshop.RecordCustomerDecisionAsync(order.Id, order.CustomerDecisionRequests.Single().Id, approved: false);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("Rejected", body.CustomerDecisionRequests.Single().Status);
        Assert.Equal(Examples.AgreedPrice, body.CurrentAgreedPrice);
    }

    [Fact]
    public async Task PostOrdersDecision_LastPendingRequest_ReturnsOkInProgress()
    {
        // Arrange: two requests, and the first is answered
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop);
        var requests = (await (await workshop.RequestCustomerDecisionAsync(order.Id, null)).ReadOrderAsync()).CustomerDecisionRequests;
        var afterFirst = await (await workshop.RecordCustomerDecisionAsync(order.Id, requests[0].Id)).ReadOrderAsync();

        // Act
        var response = await workshop.RecordCustomerDecisionAsync(order.Id, requests[1].Id);

        // Assert
        Assert.Equal("WaitingForCustomer", afterFirst.Status);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("InProgress", (await response.ReadOrderAsync()).Status);
    }

    [Fact]
    public async Task PostOrdersFittings_OrderInProgress_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);

        // Act
        var response = await workshop.ScheduleFittingAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("WaitingForFitting", body.Status);
        Assert.Single(body.Fittings);
    }

    [Fact]
    public async Task PatchOrdersFittingsComplete_ScheduledFitting_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForFittingAsync(workshop);

        // Act
        var response = await workshop.CompleteFittingAsync(order.Id, order.Fittings.Single().Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal(Examples.FittingOutcome, body.Fittings.Single().Outcome);
    }

    [Fact]
    public async Task PatchOrdersFittingsComplete_LastOpenFitting_ReturnsOkInProgress()
    {
        // Arrange: two fittings, and the first is completed
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForFittingAsync(workshop);
        var fittings = (await (await workshop.ScheduleFittingAsync(order.Id)).ReadOrderAsync()).Fittings;
        var afterFirst = await (await workshop.CompleteFittingAsync(order.Id, fittings[0].Id)).ReadOrderAsync();

        // Act
        var response = await workshop.CompleteFittingAsync(order.Id, fittings[1].Id);

        // Assert
        Assert.Equal("WaitingForFitting", afterFirst.Status);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("InProgress", (await response.ReadOrderAsync()).Status);
    }

    [Fact]
    public async Task PostOrdersCommunications_IncomingEmail_ReturnsOk()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);

        // Act
        var response = await workshop.RecordCommunicationAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal(Examples.CommunicationContent, body.Communications.Single().Content);
        Assert.Contains(body.History, h => h.Type == "CommunicationRecorded");
    }

    [Fact]
    public async Task PostOrdersCommunications_Failed_ReturnsOkWithTheStatusUnchanged()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderInProgressAsync(workshop);

        // Act
        var response = await workshop.RecordCommunicationAsync(order.Id, "Outgoing", "Failed");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.ReadOrderAsync();
        Assert.Equal("InProgress", body.Status);
        Assert.Equal("Failed", body.Communications.Single().Outcome);
    }

    [Fact]
    public async Task PostOrdersApprovals_SignedForm_ReturnsCreated()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var order = await frontDesk.CreateOrderAsync();

        // Act
        var response = await frontDesk.RecordInitialApprovalAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApprovalResponse>();
        Assert.Equal("InitialOrder", body!.Scope);
        Assert.Equal(Examples.ApprovalDocumentReference, body.DocumentReference);
    }

    [Fact]
    public async Task PostOrdersInspectionFeeAgreements_SignedForm_ReturnsCreated()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var order = await frontDesk.CreateOrderAsync();

        // Act
        var response = await frontDesk.RecordInspectionFeeAgreementAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApprovalResponse>();
        Assert.Equal("InspectionFee", body!.Scope);
        Assert.Equal(Examples.InspectionFeeAmount, body.ApprovedAmount);
    }

    [Fact]
    public async Task PostOrders_WithRelatedOrder_ReturnsCreatedWithTheLink()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var earlier = await frontDesk.CreateOrderAsync();

        // Act
        var response = await frontDesk.PostOrderAsync(earlier.CustomerId, Examples.OtherWorkDescription, relatedOrderId: earlier.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(earlier.Id, body!.RelatedOrderId);
    }

    [Fact]
    public async Task PostOrdersNotes_NotInProgress_ReturnsConflict()
    {
        // Arrange: a Submitted order
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderAsync();

        // Act
        var response = await workshop.AddWorkNoteAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("The order is not in progress.", (await response.Content.ReadAsStringAsync()).Trim('"'));
    }

    [Fact]
    public async Task PatchOrdersReadyForPickup_NotInProgress_ReturnsConflict()
    {
        // Arrange: a Submitted order
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderAsync();

        // Act
        var response = await workshop.MarkReadyForPickupAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("The order is not in progress.", (await response.Content.ReadAsStringAsync()).Trim('"'));
    }

    [Fact]
    public async Task PostOrdersDecisionRequests_NotInProgress_ReturnsConflict()
    {
        // Arrange: a Submitted order
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderAsync();

        // Act
        var response = await workshop.RequestCustomerDecisionAsync(order.Id);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task PostOrdersDecision_NoWrittenResponse_ReturnsConflict()
    {
        // Arrange
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop);
        var requestId = order.CustomerDecisionRequests.Single().Id;

        // Act: an id that is not a message on the order
        var response = await workshop.PostAsJsonAsync($"/orders/{order.Id}/decision-requests/{requestId}/decision", new { communicationId = Guid.NewGuid(), approved = true });

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("Record the customer's written response first.", (await response.Content.ReadAsStringAsync()).Trim('"'));
    }

    [Fact]
    public async Task PostOrdersDecision_AlreadyAnswered_ReturnsConflict()
    {
        // Arrange: a request the customer has approved
        var frontDesk = await _factory.LogInAsFrontDeskAsync();
        var workshop = await _factory.LogInAsWorkshopAsync();
        var order = await frontDesk.CreateOrderWaitingForCustomerAsync(workshop);
        var requestId = order.CustomerDecisionRequests.Single().Id;
        (await workshop.RecordCustomerDecisionAsync(order.Id, requestId)).EnsureSuccessStatusCode();

        // Act
        var response = await workshop.RecordCustomerDecisionAsync(order.Id, requestId, approved: false);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
