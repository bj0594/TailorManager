namespace TailorManager.Core.Repository;

public interface IOrderRepository
{
    void Add(Order order);
    List<Order> GetAll();
}
