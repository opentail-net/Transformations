using Microsoft.EntityFrameworkCore;
using Smoke.Mapping.Domain;

namespace Smoke.Mapping.Data;

public sealed class SmokeDbContext : DbContext
{
    public SmokeDbContext(DbContextOptions<SmokeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<OrderLine> OrderLines => Set<OrderLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Customer>().HasKey(c => c.Id);
        modelBuilder.Entity<OrderLine>().HasKey(l => l.Id);
        modelBuilder.Entity<Order>(order =>
        {
            order.HasKey(o => o.Id);
            order.HasOne(o => o.Customer).WithMany().HasForeignKey("CustomerId");
            order.OwnsOne(o => o.ShippingAddress);
            order.HasMany(o => o.Lines).WithOne().HasForeignKey("OrderId");
        });
    }
}
