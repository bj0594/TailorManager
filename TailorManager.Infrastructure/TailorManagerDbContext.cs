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
        modelBuilder.Entity<Payment>().Property(i => i.Id).ValueGeneratedNever();
        modelBuilder.Entity<OrderHistoryEntry>().Property(e => e.Id).ValueGeneratedNever();
        modelBuilder.Entity<CustomerDecisionRequest>().Property(r => r.Id).ValueGeneratedNever();
        modelBuilder.Entity<CustomerApproval>().Property(a => a.Id).ValueGeneratedNever();
        modelBuilder.Entity<PriceAdjustment>().Property(a => a.Id).ValueGeneratedNever();
    }
}
