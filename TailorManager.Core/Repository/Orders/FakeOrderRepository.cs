namespace TailorManager.Core.Repository;

// Keeps the orders in memory, for the tests.
public class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public void Add(Order order) => _orders.Add(order);
    public List<Order> GetAll() => _orders.ToList();
    public Order? GetById(Guid id) => _orders.FirstOrDefault(o => o.Id == id);

    // The objects in the list are the ones the services change, so there is nothing to save.
    public void Update(Order order)
    {
    }
}
