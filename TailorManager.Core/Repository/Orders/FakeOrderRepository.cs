namespace TailorManager.Core.Repository;

// Keeps the orders in memory, for the tests.
public class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public void Add(Order order) => _orders.Add(order);
    public List<Order> GetAll() => _orders.Select(OldestFirst).ToList();
    public Order? GetById(Guid id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        return order is null ? null : OldestFirst(order);
    }
    public List<Order> GetAll(OrderStatus? status = null, Guid? customerId = null) =>
        _orders.Where(o => (status is null || o.Status == status)
                        && (customerId is null || o.CustomerId == customerId))
               .Select(OldestFirst).ToList();

    // The history oldest first, like the database reads it. OrderBy keeps the order of entries with the same time.
    private static Order OldestFirst(Order order)
    {
        order.History = order.History.OrderBy(h => h.OccurredAt).ToList();
        return order;
    }

    // The objects in the list are the ones the services change, so there is nothing to save.
    public void Update(Order order)
    {
    }
}
