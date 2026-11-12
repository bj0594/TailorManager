namespace TailorManager.Core.Repository;

public class FakeOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders = new();

    public void Add(Order order) => _orders.Add(order);
    public List<Order> GetAll() => _orders;
    public Order? GetById(Guid id) => _orders.FirstOrDefault(o => o.Id == id);
    public void Update(Order order)
    {
    }
}
