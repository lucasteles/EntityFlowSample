using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        var source = Entry(from);
        source.State = EntityState.Detached;
        var target = Attach(to);
        target.State = EntityState.Modified;

        foreach (var targetNav in target.Navigations)
            switch (targetNav)
            {
                case CollectionEntry {CurrentValue: { } values}:
                    foreach (var item in values)
                    {
                        var entry = Entry(item);
                        if (entry.State is EntityState.Detached)
                            Attach(item);
                    }

                    break;
            }

        // foreach (var targetNav in target.Navigations)
        //     switch (targetNav)
        //     {
        //         case ReferenceEntry entry:
        //             if (source.References.SingleOrDefault(x =>
        //                     x.Metadata.Name == entry.Metadata.Name
        //                     && x.Metadata.ClrType == entry.Metadata.ClrType
        //                 ) is { } reference)
        //                 entry.CurrentValue = reference.CurrentValue;
        //
        //             break;
        //
        //         case CollectionEntry entry:
        //             if (source.Collections.SingleOrDefault(x =>
        //                     x.Metadata.Name == entry.Metadata.Name
        //                     && x.Metadata.ClrType == entry.Metadata.ClrType
        //                 ) is { } collection)
        //                 entry.CurrentValue = collection.CurrentValue ;
        //
        //             break;
        //     }
    }
}