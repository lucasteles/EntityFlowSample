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

        order
            .HasDiscriminator(o => o.Status)
            .HasValue<Order.Pending>(Order.StatusEnum.Pending)
            .HasValue<Order.Confirmed>(Order.StatusEnum.Confirmed)
            .HasValue<Order.Cancelled>(Order.StatusEnum.Cancelled)
            .HasValue<Order.Finalized>(Order.StatusEnum.Finalized);

        order.HasMany(o => o.History).WithOne().OnDelete(DeleteBehavior.Cascade);
        order.Navigation(o => o.History).AutoInclude();

        var orderHistory = builder.Entity<OrderHistory>();
        orderHistory.Property<int>("Id").HasColumnType("int").ValueGeneratedOnAdd();
        orderHistory.HasKey("Id");
        orderHistory.Property(h => h.Photo).HasColumnType("jsonb");
    }

    public void Evolve(object from, object to)
    {
        Entry(from).State = EntityState.Detached;
        Entry(to).State = EntityState.Modified;
    }
}