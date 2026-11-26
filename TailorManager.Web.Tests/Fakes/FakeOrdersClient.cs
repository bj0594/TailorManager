using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
// Each action remembers the call, and gives the order the status the API would, so the page shows it after a reload.
// Set Fail to make the next action throw, like the API answering 409.
public class FakeOrdersClient : IOrdersClient
{
    public Exception? Fail { get; set; }
    public List<OrderDto> Items { get; } = [];

    public List<Guid> StartIds { get; } = [];
    public List<(Guid Id, string Note)> AddedWorkNotes { get; } = [];
    public List<Guid> ReadyForPickupIds { get; } = [];
    public List<(Guid Id, decimal Amount, string TransactionReference)> Payments { get; } = [];
    public List<Guid> DeliveredIds { get; } = [];
    public List<(Guid Id, string ProposedWorkChange, decimal? ProposedPriceAdjustment)> DecisionRequests { get; } = [];
    public List<(Guid Id, Guid RequestId, Guid CommunicationId, bool Approved)> Decisions { get; } = [];
    public List<(Guid Id, DateTimeOffset ScheduledAt)> ScheduledFittings { get; } = [];
    public List<(Guid Id, Guid FittingId, string Outcome)> CompletedFittings { get; } = [];
    public List<(Guid Id, CommunicationInput Communication)> RecordedCommunications { get; } = [];
    public List<(Guid Id, string Method, string? DocumentReference)> InitialApprovals { get; } = [];
    public List<(Guid Id, decimal Amount, string DocumentReference)> InspectionFeeAgreements { get; } = [];

    public Task<List<OrderDto>> GetOrdersAsync() => Task.FromResult(Items);

    public Task<OrderDto> GetOrderAsync(Guid id) => Task.FromResult(Items.Single(i => i.Id == id));

    // A new order is Submitted, with the agreed price as the current price and the balance, like the API answers.
    public Task<OrderDto> CreateOrderAsync(Guid customerId, string type, string workDescription, decimal agreedPrice, Guid? relatedOrderId = null)
    {
        if (Fail is not null) throw Fail;
        var order = new OrderDto(Guid.NewGuid(), customerId, type, workDescription, agreedPrice, agreedPrice, agreedPrice, "Submitted", DateTimeOffset.Now, relatedOrderId);
        Items.Add(order);
        return Task.FromResult(order);
    }

    public Task StartWorkAsync(Guid id) => Record(() => StartIds.Add(id), id, "InProgress");

    public Task AddWorkNoteAsync(Guid id, string note) => Record(() => AddedWorkNotes.Add((id, note)), id);

    public Task MarkReadyForPickupAsync(Guid id) => Record(() => ReadyForPickupIds.Add(id), id, "ReadyForPickup");

    public Task RequestCustomerDecisionAsync(Guid id, string proposedWorkChange, decimal? proposedPriceAdjustment) =>
        Record(() => DecisionRequests.Add((id, proposedWorkChange, proposedPriceAdjustment)), id, "WaitingForCustomer");

    // Answers the request, and adds an approved price adjustment to the price and the balance, like the API.
    public Task RecordCustomerDecisionAsync(Guid id, Guid requestId, Guid communicationId, bool approved) =>
        Record(() => Decisions.Add((id, requestId, communicationId, approved)), id, "InProgress", order => Decide(order, requestId, approved));

    public Task ScheduleFittingAsync(Guid id, DateTimeOffset scheduledAt) =>
        Record(() => ScheduledFittings.Add((id, scheduledAt)), id, "WaitingForFitting",
            order => order with { Fittings = (order.Fittings ?? []).Append(new FittingDto(Guid.NewGuid(), scheduledAt, null, null)).ToList() });

    // Writes the outcome on the fitting. The status stays, like the API until every fitting is completed.
    public Task CompleteFittingAsync(Guid id, Guid fittingId, string outcome) =>
        Record(() => CompletedFittings.Add((id, fittingId, outcome)), id, null,
            order => order with
            {
                Fittings = (order.Fittings ?? []).Select(f => f.Id == fittingId ? f with { Outcome = outcome, CompletedAt = DateTimeOffset.Now } : f).ToList()
            });

    public Task RecordCommunicationAsync(Guid id, CommunicationInput communication) =>
        Record(() => RecordedCommunications.Add((id, communication)), id, null,
            order => order with
            {
                Communications = (order.Communications ?? []).Append(new CommunicationDto(Guid.NewGuid(), communication.Channel, communication.Direction,
                    communication.Source, communication.Content, communication.Outcome, communication.OccurredAt)).ToList()
            });

    public Task RecordInitialApprovalAsync(Guid id, string method, string? documentReference) =>
        Record(() => InitialApprovals.Add((id, method, documentReference)), id, null,
            order => order with
            {
                CustomerApprovals = (order.CustomerApprovals ?? []).Append(new CustomerApprovalDto(Guid.NewGuid(), "InitialOrder", method, documentReference, order.CurrentAgreedPrice)).ToList()
            });

    public Task RecordInspectionFeeAgreementAsync(Guid id, decimal amount, string documentReference) =>
        Record(() => InspectionFeeAgreements.Add((id, amount, documentReference)), id, null,
            order => order with
            {
                CustomerApprovals = (order.CustomerApprovals ?? []).Append(new CustomerApprovalDto(Guid.NewGuid(), "InspectionFee", "SignedForm", documentReference, amount)).ToList()
            });

    public Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference) =>
        Record(() => Payments.Add((id, amount, transactionReference)), id, "Delivered");

    public Task MarkDeliveredAsync(Guid id) => Record(() => DeliveredIds.Add(id), id, "Delivered");

    // change: what else the action does to the order, like a new price.
    private Task Record(Action remember, Guid id, string? newStatus = null, Func<OrderDto, OrderDto>? change = null)
    {
        if (Fail is not null) throw Fail;
        remember();
        var index = Items.FindIndex(i => i.Id == id);
        if (index >= 0)
        {
            var order = change is null ? Items[index] : change(Items[index]);
            Items[index] = newStatus is null ? order : order with { Status = newStatus };
        }
        return Task.CompletedTask;
    }

    public Task<List<OrderDto>> ListOrdersAsync(string? status = null, Guid? customerId = null) =>
    Task.FromResult(Items
        .Where(o => status is null || o.Status == status)
        .Where(o => customerId is null || o.CustomerId == customerId)
        .ToList());

    private static OrderDto Decide(OrderDto order, Guid requestId, bool approved)
    {
        var requests = order.CustomerDecisionRequests ?? [];
        var request = requests.Single(r => r.Id == requestId);
        var adjustment = approved ? request.ProposedPriceAdjustment ?? 0m : 0m;
        return order with
        {
            CustomerDecisionRequests = requests.Select(r => r.Id == requestId ? r with { Status = approved ? "Approved" : "Rejected" } : r).ToList(),
            CurrentAgreedPrice = order.CurrentAgreedPrice + adjustment,
            OutstandingBalance = order.OutstandingBalance + adjustment
        };
    }
}
