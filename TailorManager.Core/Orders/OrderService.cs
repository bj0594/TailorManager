using TailorManager.Core.Repository;

namespace TailorManager.Core;

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICustomerRepository _customerRepository;
    private readonly TimeProvider _clock;

    // The clock is TimeProvider.System in the API, and a TestClock in the tests (§4, Dependencies).
    public OrderService(IOrderRepository repository, ICustomerRepository customerRepository, TimeProvider? clock = null)
    {
        _repository = repository;
        _customerRepository = customerRepository;
        _clock = clock ?? TimeProvider.System;
    }

    public Order AddOrder(Guid customerId, OrderType type, string workDescription, decimal agreedPrice, Guid? relatedOrderId, User employee)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Type = type,
            WorkDescription = workDescription,
            InitialAgreedPrice = agreedPrice,
            RelatedOrderId = relatedOrderId
        };
        AddHistory(order, employee, ActivityType.OrderCreated);
        _repository.Add(order);
        return order;
    }

    public List<Order> ListOrders(OrderStatus? status = null, Guid? customerId = null)
    {
        var query = _repository.GetAll().AsEnumerable();
        if (status is not null) query = query.Where(o => o.Status == status);
        if (customerId is not null) query = query.Where(o => o.CustomerId == customerId);
        return query.ToList();
    }
    public Order GetOrder(Guid id) =>
    _repository.GetById(id) ?? throw new KeyNotFoundException($"Order {id} was not found.");

    public Order StartOrder(Guid id, User employee)
    {
        var order = GetOrder(id);
        if (order.Status != OrderStatus.Submitted)
        {
            throw new InvalidOrderStatusException("The order is not Submitted.");
        }
        MoveTo(order, OrderStatus.InProgress, employee);
        return Save(order);
    }

    public Order AddWorkNote(Guid id, string note, User employee)
    {
        var order = GetOrder(id);
        AddHistory(order, employee, ActivityType.WorkNoteAdded, note);
        return Save(order);
    }

    // The workshop asks the customer about a change (§2). The price stays until the customer approves it (RecordCustomerDecision).
    public Order RequestCustomerDecision(Guid id, string proposedWorkChange, decimal? proposedPriceAdjustment, User employee)
    {
        var order = GetOrder(id);
        var request = new CustomerDecisionRequest
        {
            Id = Guid.NewGuid(),
            ProposedWorkChange = proposedWorkChange,
            ProposedPriceAdjustment = proposedPriceAdjustment
        };
        order.CustomerDecisionRequests.Add(request);
        AddHistory(order, employee, ActivityType.CustomerDecisionRequested, relatedEntityId: request.Id);

        // The customer is told once, when the order starts waiting (§4).
        if (order.Status != OrderStatus.WaitingForCustomer)
        {
            MoveTo(order, OrderStatus.WaitingForCustomer, employee);
            AddHistory(order, employee, ActivityType.NotificationRequested);
        }
        return Save(order);
    }

    // The customer's written answer to a request (§2).
    public Order RecordCustomerDecision(Guid id, Guid requestId, Guid communicationId, bool approved, User employee)
    {
        var order = GetOrder(id);
        var request = order.CustomerDecisionRequests.Single(r => r.Id == requestId);
        request.Status = approved ? DecisionStatus.Approved : DecisionStatus.Rejected;
        request.ResponseCommunicationId = communicationId;
        request.RespondedAt = _clock.GetUtcNow();
        AddHistory(order, employee, ActivityType.CustomerDecisionRecorded, relatedEntityId: request.Id);

        // A rejected change keeps the agreed work and price (§5, Reject a change).
        if (approved)
        {
            ApproveChange(order, request, employee);
        }

        // The work goes on when the customer has answered every request (§3, Order status).
        if (order.CustomerDecisionRequests.All(r => r.Status != DecisionStatus.Pending))
        {
            MoveTo(order, OrderStatus.InProgress, employee);
        }
        return Save(order);
    }

    // An approved change is a CustomerApproval, and its price adjustment is part of CurrentAgreedPrice from now on.
    // The method is Email until the communication it came in is recorded (§5, Record communication).
    private void ApproveChange(Order order, CustomerDecisionRequest request, User employee)
    {
        var approval = new CustomerApproval
        {
            Id = Guid.NewGuid(),
            Scope = ApprovalScope.WorkChange,
            Method = ApprovalMethod.Email,
            ApprovedAt = request.RespondedAt!.Value,
            RelatedDecisionRequestId = request.Id,
            CommunicationId = request.ResponseCommunicationId,
            ApprovedAmount = request.ProposedPriceAdjustment,
            ApprovedTerms = request.ProposedWorkChange
        };
        order.CustomerApprovals.Add(approval);

        // A change without a price keeps the agreed price (§5, Approve a change without a price).
        if (request.ProposedPriceAdjustment is not { } amount)
        {
            return;
        }
        var adjustment = new PriceAdjustment
        {
            Id = Guid.NewGuid(),
            Description = request.ProposedWorkChange,
            Amount = amount,
            ApprovedAt = approval.ApprovedAt,
            CustomerApprovalId = approval.Id
        };
        order.ApprovedPriceAdjustments.Add(adjustment);
        AddHistory(order, employee, ActivityType.PriceAdjusted, relatedEntityId: adjustment.Id);
    }

    // A fitting with the customer. The order waits until every fitting is completed (§3, Order status).
    public Order ScheduleFitting(Guid id, DateTimeOffset scheduledAt, User employee)
    {
        var order = GetOrder(id);
        var fitting = new FittingAppointment { Id = Guid.NewGuid(), ScheduledAt = scheduledAt };
        order.Fittings.Add(fitting);
        AddHistory(order, employee, ActivityType.FittingScheduled, relatedEntityId: fitting.Id);

        // A second fitting while waiting adds the appointment, and the status stays (§4).
        if (order.Status != OrderStatus.WaitingForFitting)
        {
            MoveTo(order, OrderStatus.WaitingForFitting, employee);
        }
        return Save(order);
    }

    // The outcome of a fitting, written by the employee after it.
    public Order CompleteFitting(Guid id, Guid fittingId, string outcome, User employee)
    {
        var order = GetOrder(id);
        var fitting = order.Fittings.Single(f => f.Id == fittingId);
        fitting.Outcome = outcome;
        fitting.CompletedAt = _clock.GetUtcNow();
        AddHistory(order, employee, ActivityType.FittingCompleted, outcome, fitting.Id);

        // The work goes on when every fitting is completed (§3, Order status).
        if (order.Fittings.All(f => f.CompletedAt is not null))
        {
            MoveTo(order, OrderStatus.InProgress, employee);
        }
        return Save(order);
    }

    // The customer is told the order is ready: a notification request in the history (§4). Nothing is sent from Core.
    public Order MarkReadyForPickup(Guid id, User employee)
    {
        var order = GetOrder(id);
        MoveTo(order, OrderStatus.ReadyForPickup, employee);
        AddHistory(order, employee, ActivityType.NotificationRequested);
        return Save(order);
    }

    // A full payment delivers the order (§2).
    public Order RegisterPayment(Guid id, decimal amount, string transactionReference, User employee)
    {
        var order = GetOrder(id);
        order.Payments.Add(new Payment { Id = Guid.NewGuid(), Amount = amount, TransactionReference = transactionReference });
        AddHistory(order, employee, ActivityType.PaymentReceived);
        MoveTo(order, OrderStatus.Delivered, employee);
        return Save(order);
    }

    public Order MarkDelivered(Guid id, User employee)
    {
        var order = GetOrder(id);
        MoveTo(order, OrderStatus.Delivered, employee);
        return Save(order);
    }

    // A new status, with a StatusChanged line in the history.
    private void MoveTo(Order order, OrderStatus status, User employee)
    {
        order.Status = status;
        AddHistory(order, employee, ActivityType.StatusChanged);
    }

    // A new line in the history, by the employee who did it, at the time on the clock. The history is append-only.
    private void AddHistory(Order order, User employee, ActivityType type, string? note = null, Guid? relatedEntityId = null)
    {
        var entry = OrderHistoryEntry.By(employee);
        entry.Type = type;
        entry.Note = note;
        entry.RelatedEntityId = relatedEntityId;
        entry.OccurredAt = _clock.GetUtcNow();
        order.History.Add(entry);
    }

    private Order Save(Order order)
    {
        _repository.Update(order);
        return order;
    }
}
