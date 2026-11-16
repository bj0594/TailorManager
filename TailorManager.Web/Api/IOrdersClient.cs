namespace TailorManager.Web.Api;

// What the pages can ask the API for. OrdersClient calls the API, and FakeOrdersClient is used in the tests.
public interface IOrdersClient
{
    Task<List<OrderDto>> GetOrdersAsync();
}
