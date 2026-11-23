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

    public List<Order> GetAll() => WithLists().ToList();

    public Order? GetById(Guid id) => WithLists().FirstOrDefault(o => o.Id == id);

    public void Update(Order order)
    {
        if (_db.Entry(order).State == EntityState.Detached)
            _db.Orders.Update(order);
        _db.SaveChanges();
    }

    // The orders with the lists on them (history and payments), so they are read from the database too.
    private IQueryable<Order> WithLists() => _db.Orders.Include(o => o.History).Include(o => o.Payments);
}
