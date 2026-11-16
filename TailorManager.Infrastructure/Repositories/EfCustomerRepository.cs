using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Infrastructure;

public class EfCustomerRepository : ICustomerRepository
{
    private readonly TailorManagerDbContext _db;

    public EfCustomerRepository(TailorManagerDbContext db)
    {
        _db = db;
    }

    public void Add(Customer customer)
    {
        _db.Customers.Add(customer);
        _db.SaveChanges();
    }

    public List<Customer> GetAll() => _db.Customers.ToList();

    public Customer? GetById(Guid id) => _db.Customers.Find(id);

    public void Update(Customer customer)
    {
        _db.Customers.Update(customer);
        _db.SaveChanges();
    }
}
