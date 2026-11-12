using Microsoft.EntityFrameworkCore;
using TailorManager.Core;

namespace TailorManager.Infrastructure;

public class TailorManagerDbContext : DbContext
{
    public TailorManagerDbContext(DbContextOptions<TailorManagerDbContext> options) : base(options) { }

public DbSet<Customer> Customers => Set<Customer>();
public DbSet<Order> Orders => Set<Order>();
}
