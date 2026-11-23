using TailorManager.Core;
using TailorManager.Core.Repository;
using Microsoft.EntityFrameworkCore;

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

    public Order? GetById(Guid id) =>
        _db.Orders.Include(t => t.History).Include(t => t.Payments).FirstOrDefault(t => t.Id == id);

    public void Update(Order order)
    {
        if (_db.Entry(order).State == EntityState.Detached)
            _db.Orders.Update(order);
        _db.SaveChanges();
    }
}
