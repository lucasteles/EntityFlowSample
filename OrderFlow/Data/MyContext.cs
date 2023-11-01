using Microsoft.EntityFrameworkCore;
using OrderFlow.Models;

namespace OrderFlow.Data;

public class MyContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        var order = builder.Entity<Order>();
        order.HasKey(o => o.Id);
        order.Property(o => o.Id).HasConversion(x => x.Value, x => new(x));

        order
            .HasDiscriminator(o => o.Status)
            .HasValue<Order.Pending>(Order.StatusEnum.Pending)
            .HasValue<Order.Confirmed>(Order.StatusEnum.Confirmed)
            .HasValue<Order.Cancelled>(Order.StatusEnum.Cancelled)
            .HasValue<Order.Finalized>(Order.StatusEnum.Finalized)
            ;
    }

    public void Evolve(object from, object to)
    {
        Entry(from).State = EntityState.Detached;
        Entry(to).State = EntityState.Modified;
    }

    public async Task<IDictionary<string, object?>> Query(string sqlCommand)
    {
        await using var command = Database.GetDbConnection().CreateCommand();
        command.CommandText = sqlCommand;
        await Database.OpenConnectionAsync();
        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<string, object?> result = new();
        while (await reader.ReadAsync())
            for (var i = 0; i < reader.FieldCount; i++)
                result.Add(reader.GetName(i), reader.GetValue(i));
        return result;
    }
}