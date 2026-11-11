namespace TailorManager.Core.Repository;

public class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public void Add(Order order) => _orders.Add(order);
    public List<Order> GetAll() => _orders;
}
