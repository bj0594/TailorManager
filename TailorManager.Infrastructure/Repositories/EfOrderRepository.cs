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

    public List<Order> GetAll(OrderStatus? status = null, Guid? customerId = null)
    {
        var query = WithLists();
        if (status is not null) query = query.Where(o => o.Status == status);
        if (customerId is not null) query = query.Where(o => o.CustomerId == customerId);
        return query.ToList();
    }

    // The orders with the lists on them, so they are read from the database too.
    // The history is read oldest first (§5, Order history).
    private IQueryable<Order> WithLists() => _db.Orders
        .Include(o => o.History.OrderBy(h => h.OccurredAt))
        .Include(o => o.Payments)
        .Include(o => o.CustomerDecisionRequests)
        .Include(o => o.CustomerApprovals)
        .Include(o => o.ApprovedPriceAdjustments);
}
