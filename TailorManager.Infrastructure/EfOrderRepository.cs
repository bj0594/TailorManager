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
        throw new NotImplementedException();
    }

    public List<Order> GetAll() => throw new NotImplementedException();

    public Order? GetById(Guid id) => throw new NotImplementedException();

    public void Update(Order order)
    {
        throw new NotImplementedException();
    }
}
