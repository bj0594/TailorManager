using TailorManager.Web.Api;

namespace TailorManager.Web.Tests;

// Answers like the API, without HTTP. The tests fill Items with what the page should get.
public class FakeOrdersClient : IOrdersClient
{
    public List<OrderDto> Items { get; } = [];

    public Task<List<OrderDto>> GetOrdersAsync() => Task.FromResult(Items);
}
