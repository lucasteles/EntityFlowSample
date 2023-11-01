using Microsoft.EntityFrameworkCore;
using OrderFlow.Models;

namespace OrderFlow;

public static class Extensions
{
    public static DateTime UtcNow(this TimeProvider time) =>
        time.GetUtcNow().UtcDateTime;

    public static ValueTask<T?> FindById<T>
        (this DbSet<T> db, OrderId id)
        where T : Order =>
        db.FindAsync(id);
}