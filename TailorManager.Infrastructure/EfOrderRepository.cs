using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Infrastructure;

public class EfOrderRepository : IOrderRepository
{
    private readonly TailorManagerDbContext _db;

    public EfOrderRepository(TailorManagerDbContext db)
    {
        _db = db;
    }

    public void Add(Order order)
    {
        _db.Orders.Add(order);
        _db.SaveChanges();
    }

    public List<Order> GetAll() => _db.Orders.ToList();

    public Order? GetById(Guid id) => _db.Orders.Find(id);

    public void Update(Order order)
    {
        _db.Orders.Update(order);
        _db.SaveChanges();
    }
}
