using TailorManager.Core;
using TailorManager.Core.Repository;

namespace TailorManager.Infrastructure;

public class EfUserRepository : IUserRepository
{
    private readonly TailorManagerDbContext _db;

    public EfUserRepository(TailorManagerDbContext db)
    {
        _db = db;
    }

    public void Add(User user) => throw new NotImplementedException();

    public User? GetByUsername(string username) => throw new NotImplementedException();
}
