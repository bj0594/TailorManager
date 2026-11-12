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
        throw new NotImplementedException();
    }

    public List<Customer> GetAll() => throw new NotImplementedException();

    public Customer? GetById(Guid id) => throw new NotImplementedException();

    public void Update(Customer customer)
    {
        throw new NotImplementedException();
    }
}
