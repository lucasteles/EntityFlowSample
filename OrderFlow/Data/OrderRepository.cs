using Microsoft.EntityFrameworkCore;
using OrderFlow.Models;

namespace OrderFlow.Data;

public sealed class OrderRepository(MyContext db)
{
    public DbSet<Order> Orders => db.Orders;
    public IQueryable<Order.Pending> Pending => db.Set<Order.Pending>();
    public IQueryable<Order.Confirmed> Confirmed => db.Set<Order.Confirmed>();
    public IQueryable<Order.Cancelled> Cancelled => db.Set<Order.Cancelled>();
    public IQueryable<Order.Finalized> Finalized => db.Set<Order.Finalized>();

    public void Evolve(Order from, Order to) => db.Evolve(from, to);
    public Task Save() => db.SaveChangesAsync();
}