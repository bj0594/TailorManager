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

    public Task<List<OrderDto>> GetOrdersAsync() => Task.FromResult(Items);

    public Task<OrderDto> GetOrderAsync(Guid id) => Task.FromResult(Items.Single(i => i.Id == id));

    // A new order is Submitted, with the agreed price as the current price and the balance, like the API answers.
    public Task<OrderDto> CreateOrderAsync(Guid customerId, string type, string workDescription, decimal agreedPrice)
    {
        if (Fail is not null) throw Fail;
        var order = new OrderDto(Guid.NewGuid(), customerId, type, workDescription, agreedPrice, agreedPrice, agreedPrice, "Submitted", DateTimeOffset.Now, null);
        Items.Add(order);
        return Task.FromResult(order);
    }

    public Task StartWorkAsync(Guid id) => Record(() => StartIds.Add(id), id, "InProgress");

    public Task AddWorkNoteAsync(Guid id, string note) => Record(() => AddedWorkNotes.Add((id, note)), id);

    public Task MarkReadyForPickupAsync(Guid id) => Record(() => ReadyForPickupIds.Add(id), id, "ReadyForPickup");

    public Task RegisterPaymentAsync(Guid id, decimal amount, string transactionReference) =>
        Record(() => Payments.Add((id, amount, transactionReference)), id, "Delivered");

    public Task MarkDeliveredAsync(Guid id) => Record(() => DeliveredIds.Add(id), id, "Delivered");

    private Task Record(Action remember, Guid id, string? newStatus = null)
    {
        if (Fail is not null) throw Fail;
        remember();
        var index = Items.FindIndex(i => i.Id == id);
        if (newStatus is not null && index >= 0)
        {
            Items[index] = Items[index] with { Status = newStatus };
        }
        return Task.CompletedTask;
    }

    public Task<List<OrderDto>> ListOrdersAsync(string? status = null, Guid? customerId = null) =>
    Task.FromResult(Items
        .Where(o => status is null || o.Status == status)
        .Where(o => customerId is null || o.CustomerId == customerId)
        .ToList());
}
