using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Models;

namespace OrderFlow;

public class MyContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var order = builder.Entity<Order>();
        order.HasKey(o => o.Id);
        order.Property(o => o.Id).HasConversion(x => x.Value, x => new(x));
        order.Metadata.FindNavigation(nameof(Order.History))
            ?.SetPropertyAccessMode(PropertyAccessMode.Field);

        order.OwnsMany(x => x.History);
        order
            .HasDiscriminator(o => o.Status)
            .HasValue<Order.Pending>(Order.StatusEnum.Pending)
            .HasValue<Order.Confirmed>(Order.StatusEnum.Confirmed)
            .HasValue<Order.Cancelled>(Order.StatusEnum.Cancelled)
            .HasValue<Order.Finalized>(Order.StatusEnum.Finalized)
            ;

        var orderHistory = builder.Entity<OrderHistory>();
        orderHistory.HasKey("Id");
        orderHistory.Property<int>("Id")
            .HasColumnType("int").ValueGeneratedOnAdd();

        orderHistory.Property(h => h.Photo).HasColumnType("jsonb");
    }

    public void Evolve(object from, object to)
    {
        Entry(from).State = EntityState.Detached;
        Entry(to).State = EntityState.Modified;
    }
}