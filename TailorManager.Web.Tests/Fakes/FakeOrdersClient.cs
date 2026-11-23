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
}
