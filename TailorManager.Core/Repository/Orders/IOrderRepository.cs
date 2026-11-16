namespace TailorManager.Core.Repository;

public interface IOrderRepository
{
    void Add(Order order);
    List<Order> GetAll();
    Order? GetById(Guid id);
    void Update(Order order);
}
