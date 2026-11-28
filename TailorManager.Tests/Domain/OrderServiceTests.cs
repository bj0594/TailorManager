using TailorManager.Core;

namespace TailorManager.Tests;

public class OrderServiceTests
{
    // The services with fake repositories, a customer and an employee ready to use (TestData/FakeServices.cs).
    private readonly FakeServices _app = new();
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _service = _app.Orders;
    }

    [Fact]
    public void AddOrder_ValidInput_ReturnsTheOrder()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, Examples.WorkDescription, Examples.AgreedPrice, null, _app.Employee);

        // Assert
        Assert.Equal(Examples.WorkDescription, order.WorkDescription);
        Assert.Equal(Examples.AgreedPrice, order.InitialAgreedPrice);
        Assert.Equal(OrderStatus.Submitted, order.Status);
        Assert.Equal(ActivityType.OrderCreated, Assert.Single(order.History).Type);
    }

    [Fact]
    public void AddOrder_WithEmployee_StoresTheEmployeeInTheHistory()
    {
        // Arrange
        var customer = _app.AddCustomer();

        // Act
        var order = _service.AddOrder(customer.Id, OrderType.NewGarment, Examples.WorkDescription, Examples.AgreedPrice, null, _app.Employee);

        // Assert
        Assert.Equal(Examples.EmployeeDisplayName, order.History.First().ActorDisplayName);
    }

    [Fact]
    public void GetOrder_ExistingId_ReturnsThatOrder()
    {
        // Arrange
        _app.AddOrder(workDescription: Examples.OtherWorkDescription);
        var order = _app.AddOrder();

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Equal(order.Id, found.Id);
    }

    [Fact]
    public void StartOrder_ExistingOrder_SetsInProgress()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        _service.StartOrder(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.InProgress, _service.GetOrder(order.Id).Status);
    }

    [Fact]
    public void StartOrder_ExistingOrder_AddsStatusChangedEntry()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        var result = _service.StartOrder(order.Id, _app.Employee);

        // Assert
        var entry = Assert.Single(result.History, h => h.Type == ActivityType.StatusChanged);
        Assert.Equal(Examples.EmployeeDisplayName, entry.ActorDisplayName);
    }

    [Fact]
    public void StartOrder_NotSubmitted_ThrowsInvalidOrderStatusException()
    {
        // Arrange
        var order = _app.AddOrderInProgress();

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.StartOrder(order.Id, _app.Employee));
    }

    [Fact]
    public void AddWorkNote_OrderInProgress_AddsWorkNote()
    {
        // Arrange
        var order = _app.AddOrderInProgress();

        // Act
        _service.AddWorkNote(order.Id, Examples.WorkNote, _app.Employee);

        // Assert
        var entry = Assert.Single(_service.GetOrder(order.Id).History, h => h.Type == ActivityType.WorkNoteAdded);
        Assert.Equal(Examples.WorkNote, entry.Note);
    }

    [Fact]
    public void MarkReadyForPickup_OrderInProgress_SetsReadyForPickup()
    {
        // Arrange
        var order = _app.AddOrderInProgress();

        // Act
        var result = _service.MarkReadyForPickup(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.ReadyForPickup, result.Status);
        Assert.Equal(2, result.History.Count(h => h.Type == ActivityType.StatusChanged));
        Assert.Single(result.History, h => h.Type == ActivityType.NotificationRequested);
    }

    [Fact]
    public void RegisterPayment_FullPayment_RecordsThePaymentAndDelivers()
    {
        // Arrange
        var order = _app.AddOrderReadyForPickup();

        // Act
        var result = _service.RegisterPayment(order.Id, Examples.PaymentAmount, Examples.TransactionReference, _app.Employee);

        // Assert
        var payment = Assert.Single(result.Payments);
        Assert.Equal(Examples.TransactionReference, payment.TransactionReference);
        Assert.Equal(0m, result.OutstandingBalance);
        Assert.Equal(OrderStatus.Delivered, result.Status);
        Assert.Single(result.History, h => h.Type == ActivityType.PaymentReceived);
    }

    // Nothing is paid yet, so the balance is the agreed price: zero for a free order (§4 Calculated values).
    public static TheoryData<decimal> AgreedPrices => new() { 0m, Examples.AgreedPrice };

    [Theory]
    [MemberData(nameof(AgreedPrices))]
    public void AddOrder_NothingPaid_OutstandingBalanceIsTheAgreedPrice(decimal agreedPrice)
    {
        // Act
        var order = _app.AddOrder(agreedPrice: agreedPrice);

        // Assert
        Assert.Equal(agreedPrice, order.OutstandingBalance);
    }

    [Fact]
    public void MarkDelivered_FreeOrderReadyForPickup_SetsDelivered()
    {
        // Arrange
        var order = _app.AddOrderReadyForPickup(agreedPrice: 0m);

        // Act
        var result = _service.MarkDelivered(order.Id, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.Delivered, result.Status);
        Assert.Equal(3, result.History.Count(h => h.Type == ActivityType.StatusChanged));
    }

    [Fact]
    public void ListOrders_NoFilter_ReturnsAll()
    {
        // Arrange
        _app.AddOrder();
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(null, null);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ListOrders_WithStatus_ReturnsOnlyMatching()
    {
        // Arrange
        _app.AddOrderInProgress();
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(OrderStatus.InProgress, null);

        // Assert
        var found = Assert.Single(result);
        Assert.Equal(OrderStatus.InProgress, found.Status);
    }

    [Fact]
    public void ListOrders_WithCustomerId_ReturnsOnlyMatching()
    {
        // Arrange
        var customer = _app.AddCustomer();
        _app.AddOrder(customer.Id);
        _app.AddOrder();

        // Act
        var result = _service.ListOrders(null, customer.Id);

        // Assert
        var found = Assert.Single(result);
        Assert.Equal(customer.Id, found.CustomerId);
    }

    [Fact]
    public void GetOrder_TwoActions_ReturnsTheHistoryOldestFirstWithTheTime()
    {
        // Arrange: the order is made now, and started an hour later
        var createdAt = _app.Clock.Now;
        var order = _app.AddOrder();
        _app.Clock.Advance(TimeSpan.FromHours(1));
        _service.StartOrder(order.Id, _app.Employee);

        // Act
        var found = _service.GetOrder(order.Id);

        // Assert
        Assert.Collection(found.History,
            entry =>
            {
                Assert.Equal(ActivityType.OrderCreated, entry.Type);
                Assert.Equal(createdAt, entry.OccurredAt);
            },
            entry =>
            {
                Assert.Equal(ActivityType.StatusChanged, entry.Type);
                Assert.Equal(createdAt.AddHours(1), entry.OccurredAt);
            });
    }

    [Fact]
    public void RequestCustomerDecision_OrderInProgress_SetsWaitingForCustomer()
    {
        // Arrange
        var order = _app.AddOrderInProgress();

        // Act
        var changed = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.WaitingForCustomer, changed.Status);
        var request = Assert.Single(changed.CustomerDecisionRequests);
        Assert.Equal(Examples.ProposedWorkChange, request.ProposedWorkChange);
        Assert.Equal(Examples.ProposedPriceAdjustment, request.ProposedPriceAdjustment);
        Assert.Equal(DecisionStatus.Pending, request.Status);
        Assert.Contains(changed.History, h => h.Type == ActivityType.CustomerDecisionRequested && h.RelatedEntityId == request.Id);
        Assert.Contains(changed.History, h => h.Type == ActivityType.NotificationRequested);
        Assert.Equal(Examples.AgreedPrice, changed.OutstandingBalance);
    }

    [Fact]
    public void RequestCustomerDecision_AlreadyWaiting_AddsTheRequestWithoutANewNotification()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee);

        // Act
        var changed = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, null, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.WaitingForCustomer, changed.Status);
        Assert.Equal(2, changed.CustomerDecisionRequests.Count);
        Assert.Single(changed.History, h => h.Type == ActivityType.NotificationRequested);
    }

    [Fact]
    public void RecordCustomerDecision_Approved_UpdatesTheAgreedPrice()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var request = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee)
            .CustomerDecisionRequests.Single();
        var communicationId = _app.AddCustomerResponse(order.Id);

        // Act
        var changed = _service.RecordCustomerDecision(order.Id, request.Id, communicationId, true, _app.Employee);

        // Assert
        Assert.Equal(DecisionStatus.Approved, request.Status);
        Assert.Equal(communicationId, request.ResponseCommunicationId);
        var approval = Assert.Single(changed.CustomerApprovals);
        Assert.Equal(ApprovalScope.WorkChange, approval.Scope);
        Assert.Equal(request.Id, approval.RelatedDecisionRequestId);
        Assert.Equal(Examples.ProposedWorkChange, approval.ApprovedTerms);
        Assert.Equal(approval.Id, Assert.Single(changed.ApprovedPriceAdjustments).CustomerApprovalId);
        Assert.Equal(Examples.AgreedPrice + Examples.ProposedPriceAdjustment, changed.CurrentAgreedPrice);
        Assert.Equal(Examples.AgreedPrice + Examples.ProposedPriceAdjustment, changed.OutstandingBalance);
        Assert.Equal(OrderStatus.InProgress, changed.Status);
    }

    [Fact]
    public void RecordCustomerDecision_ApprovedWithoutPrice_KeepsTheAgreedPrice()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var request = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, null, _app.Employee)
            .CustomerDecisionRequests.Single();

        // Act
        var changed = _service.RecordCustomerDecision(order.Id, request.Id, _app.AddCustomerResponse(order.Id), true, _app.Employee);

        // Assert
        Assert.Equal(DecisionStatus.Approved, request.Status);
        Assert.Single(changed.CustomerApprovals);
        Assert.Empty(changed.ApprovedPriceAdjustments);
        Assert.Equal(Examples.AgreedPrice, changed.CurrentAgreedPrice);
    }

    [Fact]
    public void RecordCustomerDecision_Rejected_KeepsTheAgreedWorkAndPrice()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var request = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee)
            .CustomerDecisionRequests.Single();

        // Act
        var changed = _service.RecordCustomerDecision(order.Id, request.Id, _app.AddCustomerResponse(order.Id), false, _app.Employee);

        // Assert
        Assert.Equal(DecisionStatus.Rejected, request.Status);
        Assert.Empty(changed.CustomerApprovals);
        Assert.Empty(changed.ApprovedPriceAdjustments);
        Assert.Equal(Examples.AgreedPrice, changed.CurrentAgreedPrice);
        Assert.Equal(Examples.WorkDescription, changed.WorkDescription);
    }

    [Fact]
    public void RecordCustomerDecision_AnotherRequestPending_KeepsWaitingForCustomer()
    {
        // Arrange: two requests, and the customer answers the first
        var order = _app.AddOrderInProgress();
        _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee);
        var requests = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, null, _app.Employee).CustomerDecisionRequests;

        // Act
        var afterFirst = _service.RecordCustomerDecision(order.Id, requests[0].Id, _app.AddCustomerResponse(order.Id), true, _app.Employee).Status;
        var afterSecond = _service.RecordCustomerDecision(order.Id, requests[1].Id, _app.AddCustomerResponse(order.Id), false, _app.Employee).Status;

        // Assert
        Assert.Equal(OrderStatus.WaitingForCustomer, afterFirst);
        Assert.Equal(OrderStatus.InProgress, afterSecond);
    }

    [Fact]
    public void ScheduleFitting_OrderInProgress_SetsWaitingForFitting()
    {
        // Arrange: the fitting is two days from now, like the example in §4
        var order = _app.AddOrderInProgress();
        var scheduledAt = _app.Clock.Now.AddDays(2);

        // Act
        var changed = _service.ScheduleFitting(order.Id, scheduledAt, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.WaitingForFitting, changed.Status);
        var fitting = Assert.Single(changed.Fittings);
        Assert.Equal(scheduledAt, fitting.ScheduledAt);
        Assert.Null(fitting.CompletedAt);
        Assert.Contains(changed.History, h => h.Type == ActivityType.FittingScheduled && h.RelatedEntityId == fitting.Id);
    }

    [Fact]
    public void CompleteFitting_ScheduledFitting_RecordsTheOutcome()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var fitting = _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(2), _app.Employee).Fittings.Single();
        _app.Clock.Advance(TimeSpan.FromDays(2));

        // Act
        var changed = _service.CompleteFitting(order.Id, fitting.Id, Examples.FittingOutcome, _app.Employee);

        // Assert
        var completed = Assert.Single(changed.Fittings);
        Assert.Equal(Examples.FittingOutcome, completed.Outcome);
        Assert.Equal(_app.Clock.Now, completed.CompletedAt);
        Assert.Contains(changed.History, h => h.Type == ActivityType.FittingCompleted && h.RelatedEntityId == fitting.Id);
    }

    [Fact]
    public void CompleteFitting_AnotherFittingOpen_KeepsWaitingForFitting()
    {
        // Arrange: two fittings
        var order = _app.AddOrderInProgress();
        _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(2), _app.Employee);
        var fittings = _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(4), _app.Employee).Fittings;

        // Act
        var afterFirst = _service.CompleteFitting(order.Id, fittings[0].Id, Examples.FittingOutcome, _app.Employee).Status;
        var afterSecond = _service.CompleteFitting(order.Id, fittings[1].Id, Examples.FittingOutcome, _app.Employee).Status;

        // Assert
        Assert.Equal(OrderStatus.WaitingForFitting, afterFirst);
        Assert.Equal(OrderStatus.InProgress, afterSecond);
    }

    [Theory]
    [InlineData(CommunicationChannel.Email, CommunicationDirection.Incoming)]
    [InlineData(CommunicationChannel.SMS, CommunicationDirection.Outgoing)]
    public void RecordCommunication_Message_AddsItToTheHistory(CommunicationChannel channel, CommunicationDirection direction)
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var communication = TestData.NewCommunication(channel, direction);

        // Act
        var changed = _service.RecordCommunication(order.Id, communication, _app.Employee);

        // Assert
        var recorded = Assert.Single(changed.Communications);
        Assert.Equal(channel, recorded.Channel);
        Assert.Equal(direction, recorded.Direction);
        Assert.Contains(changed.History, h => h.Type == ActivityType.CommunicationRecorded && h.RelatedEntityId == recorded.Id);
    }

    [Fact]
    public void RecordCommunication_Failed_KeepsTheStatus()
    {
        // Arrange
        var order = _app.AddOrderInProgress();
        var failed = TestData.NewCommunication(CommunicationChannel.SMS, CommunicationDirection.Outgoing, CommunicationOutcome.Failed);

        // Act
        var changed = _service.RecordCommunication(order.Id, failed, _app.Employee);

        // Assert
        Assert.Equal(OrderStatus.InProgress, changed.Status);
        Assert.Equal(CommunicationOutcome.Failed, Assert.Single(changed.Communications).Outcome);
    }

    [Fact]
    public void RecordInitialApproval_SignedForm_RecordsTheApproval()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        var approval = _service.RecordInitialApproval(order.Id, ApprovalMethod.SignedForm, Examples.ApprovalDocumentReference, _app.Employee);

        // Assert
        Assert.Equal(ApprovalScope.InitialOrder, approval.Scope);
        Assert.Equal(ApprovalMethod.SignedForm, approval.Method);
        Assert.Equal(Examples.ApprovalDocumentReference, approval.DocumentReference);
        Assert.Equal(Examples.AgreedPrice, approval.ApprovedAmount);
        var found = _service.GetOrder(order.Id);
        Assert.Contains(approval, found.CustomerApprovals);
        Assert.Contains(found.History, h => h.Type == ActivityType.InitialApprovalRecorded && h.RelatedEntityId == approval.Id);
    }

    [Fact]
    public void RecordInspectionFeeAgreement_SignedForm_RecordsTheAmountAndReference()
    {
        // Arrange
        var order = _app.AddOrder();

        // Act
        var approval = _service.RecordInspectionFeeAgreement(order.Id, Examples.InspectionFeeAmount, Examples.InspectionFeeDocumentReference, _app.Employee);

        // Assert
        Assert.Equal(ApprovalScope.InspectionFee, approval.Scope);
        Assert.Equal(Examples.InspectionFeeAmount, approval.ApprovedAmount);
        Assert.Equal(Examples.InspectionFeeDocumentReference, approval.DocumentReference);
        var found = _service.GetOrder(order.Id);
        Assert.Contains(approval, found.CustomerApprovals);
        Assert.Contains(found.History, h => h.Type == ActivityType.InspectionFeeAgreementRecorded && h.RelatedEntityId == approval.Id);
        Assert.Equal(Examples.AgreedPrice, found.CurrentAgreedPrice);
    }

    [Fact]
    public void AddOrder_EarlierOrderFromTheSameCustomer_LinksIt()
    {
        // Arrange: a repair of an earlier order
        var earlier = _app.AddOrder();

        // Act
        var order = _service.AddOrder(earlier.CustomerId, OrderType.Repair, Examples.OtherWorkDescription, Examples.AgreedPrice, earlier.Id, _app.Employee);

        // Assert
        Assert.Equal(earlier.Id, _service.GetOrder(order.Id).RelatedOrderId);
    }

    [Fact]
    public void AddWorkNote_NotInProgress_ThrowsInvalidOrderStatusException()
    {
        // Arrange: a Submitted order
        var order = _app.AddOrder();

        // Act + Assert
        var error = Assert.Throws<InvalidOrderStatusException>(() => _service.AddWorkNote(order.Id, Examples.WorkNote, _app.Employee));
        Assert.Equal("The order is not in progress.", error.Message);
    }

    [Fact]
    public void MarkReadyForPickup_NotInProgress_ThrowsInvalidOrderStatusException()
    {
        // Arrange: a Submitted order
        var order = _app.AddOrder();

        // Act + Assert
        var error = Assert.Throws<InvalidOrderStatusException>(() => _service.MarkReadyForPickup(order.Id, _app.Employee));
        Assert.Equal("The order is not in progress.", error.Message);
    }

    [Fact]
    public void RequestCustomerDecision_NotInProgress_ThrowsInvalidOrderStatusException()
    {
        // Arrange: a Submitted order
        var order = _app.AddOrder();

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() =>
            _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee));
        Assert.Empty(_service.GetOrder(order.Id).CustomerDecisionRequests);
    }

    [Fact]
    public void RecordCustomerDecision_NoWrittenResponse_ThrowsInvalidOperationException()
    {
        // Arrange: a request, and an id that is not a message on the order
        var order = _app.AddOrderInProgress();
        var request = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee)
            .CustomerDecisionRequests.Single();

        // Act + Assert
        var error = Assert.Throws<InvalidOperationException>(() =>
            _service.RecordCustomerDecision(order.Id, request.Id, Guid.NewGuid(), true, _app.Employee));
        Assert.Equal("Record the customer's written response first.", error.Message);
        Assert.Equal(DecisionStatus.Pending, request.Status);
    }

    [Fact]
    public void RecordCustomerDecision_AlreadyAnswered_ThrowsAndChangesNothing()
    {
        // Arrange: a request the customer has approved
        var order = _app.AddOrderInProgress();
        var request = _service.RequestCustomerDecision(order.Id, Examples.ProposedWorkChange, Examples.ProposedPriceAdjustment, _app.Employee)
            .CustomerDecisionRequests.Single();
        _service.RecordCustomerDecision(order.Id, request.Id, _app.AddCustomerResponse(order.Id), true, _app.Employee);
        var secondResponse = _app.AddCustomerResponse(order.Id);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() =>
            _service.RecordCustomerDecision(order.Id, request.Id, secondResponse, false, _app.Employee));
        var found = _service.GetOrder(order.Id);
        Assert.Equal(DecisionStatus.Approved, request.Status);
        Assert.Single(found.CustomerApprovals);
        Assert.Equal(Examples.AgreedPrice + Examples.ProposedPriceAdjustment, found.CurrentAgreedPrice);
    }

    [Fact]
    public void ScheduleFitting_NotInProgress_ThrowsInvalidOrderStatusException()
    {
        // Arrange: a Submitted order
        var order = _app.AddOrder();

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(2), _app.Employee));
        Assert.Empty(_service.GetOrder(order.Id).Fittings);
    }

    [Fact]
    public void CompleteFitting_OrderNotWaiting_ThrowsInvalidOrderStatusException()
    {
        // Arrange: an order in progress, with no fitting
        var order = _app.AddOrderInProgress();

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.CompleteFitting(order.Id, Guid.NewGuid(), Examples.FittingOutcome, _app.Employee));
    }

    [Fact]
    public void CompleteFitting_AlreadyCompleted_ThrowsAndKeepsTheFirstOutcome()
    {
        // Arrange: two fittings, so the order still waits after the first is completed
        var order = _app.AddOrderInProgress();
        _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(2), _app.Employee);
        var fittings = _service.ScheduleFitting(order.Id, _app.Clock.Now.AddDays(4), _app.Employee).Fittings;
        _service.CompleteFitting(order.Id, fittings[0].Id, Examples.FittingOutcome, _app.Employee);

        // Act + Assert
        Assert.Throws<InvalidOperationException>(() => _service.CompleteFitting(order.Id, fittings[0].Id, "Another outcome", _app.Employee));
        Assert.Equal(Examples.FittingOutcome, _service.GetOrder(order.Id).Fittings[0].Outcome);
    }

    [Fact]
    public void RegisterPayment_NotReadyForPickup_ThrowsInvalidOrderStatusException()
    {
        // Arrange: an order in progress
        var order = _app.AddOrderInProgress();

        // Act + Assert
        var error = Assert.Throws<InvalidOrderStatusException>(() =>
            _service.RegisterPayment(order.Id, Examples.PaymentAmount, Examples.TransactionReference, _app.Employee));
        Assert.Equal("The order is not ready for pickup.", error.Message);
        Assert.Empty(_service.GetOrder(order.Id).Payments);
    }

    [Fact]
    public void RegisterPayment_WrongAmount_ThrowsAndChangesNothing()
    {
        // Arrange
        var order = _app.AddOrderReadyForPickup();

        // Act + Assert
        var error = Assert.Throws<PaymentAmountMismatchException>(() =>
            _service.RegisterPayment(order.Id, Examples.PaymentAmount - 1, Examples.TransactionReference, _app.Employee));
        Assert.Equal("The amount is not the outstanding balance.", error.Message);
        var found = _service.GetOrder(order.Id);
        Assert.Empty(found.Payments);
        Assert.Equal(OrderStatus.ReadyForPickup, found.Status);
    }

    [Fact]
    public void RegisterPayment_TransactionAlreadyRegistered_ThrowsAndChangesNothing()
    {
        // Arrange: the transaction is already registered on another order
        var paid = _app.AddOrderReadyForPickup();
        _service.RegisterPayment(paid.Id, Examples.PaymentAmount, Examples.TransactionReference, _app.Employee);
        var order = _app.AddOrderReadyForPickup();

        // Act + Assert
        Assert.Throws<DuplicatePaymentException>(() =>
            _service.RegisterPayment(order.Id, Examples.PaymentAmount, Examples.TransactionReference, _app.Employee));
        var found = _service.GetOrder(order.Id);
        Assert.Empty(found.Payments);
        Assert.Equal(OrderStatus.ReadyForPickup, found.Status);
    }

    [Fact]
    public void MarkDelivered_NotReadyForPickup_ThrowsInvalidOrderStatusException()
    {
        // Arrange: a free order in progress
        var order = _app.AddOrderInProgress(agreedPrice: 0m);

        // Act + Assert
        Assert.Throws<InvalidOrderStatusException>(() => _service.MarkDelivered(order.Id, _app.Employee));
    }

    [Fact]
    public void MarkDelivered_OutstandingBalance_ThrowsPaymentRequiredException()
    {
        // Arrange: ready for pickup, and nothing paid
        var order = _app.AddOrderReadyForPickup();

        // Act + Assert
        var error = Assert.Throws<PaymentRequiredException>(() => _service.MarkDelivered(order.Id, _app.Employee));
        Assert.Equal("Register the payment first.", error.Message);
        Assert.Equal(OrderStatus.ReadyForPickup, _service.GetOrder(order.Id).Status);
    }
}
