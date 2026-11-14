using Microsoft.EntityFrameworkCore;
using TailorManager.Core;

namespace TailorManager.Infrastructure;

public class TailorManagerDbContext : DbContext
{
    public TailorManagerDbContext(DbContextOptions<TailorManagerDbContext> options) : base(options) { }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Username).IsUnique();
    }
}
