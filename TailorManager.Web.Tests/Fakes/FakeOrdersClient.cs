using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeOrdersClient : IOrdersClient
{
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
}
