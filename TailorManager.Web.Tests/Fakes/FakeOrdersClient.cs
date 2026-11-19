using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeOrdersClient : IOrdersClient
{
    public Exception? Fail { get; set; }
    public List<OrderDto> Items { get; } = [];

    public Task<List<OrderDto>> GetOrdersAsync() => Task.FromResult(Items);

    public Task AddOrderAsync(
        Guid Id,
        Guid CustomerId,
        string Type,
        string WorkDescription,
        decimal InitialAgreedPrice,
        decimal CurrentAgreedPrice,
        decimal OutstandingBalance,
        string Status,
        DateTimeOffset CreatedAt,
        Guid? RelatedOrderId
    )
    {
        Items.Add(new OrderDto(Guid.NewGuid(),
        CustomerId,
        Type,
        WorkDescription,
        InitialAgreedPrice,
        CurrentAgreedPrice,
        OutstandingBalance,
        Status,
        CreatedAt,
        RelatedOrderId));
        return Task.CompletedTask;
    }

    public Task<OrderDto> GetOrderAsync(Guid id) => Task.FromResult(Items.Single(i => i.Id == id));

    public List<Guid> StartIds { get; } = [];

    public Task StartOrderAsync(Guid id)
    {
        if (Fail is not null) throw Fail;
        StartIds.Add(id);
        return Task.CompletedTask;
    }

    public List<(Guid Id, string Note)> AddedWorkNotes { get; } = [];

    public Task AddWorkNoteAsync(Guid id, string Note)
    {
        AddedWorkNotes.Add((id, Note));
        return Task.CompletedTask;
    }
}
