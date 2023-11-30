using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using OrderFlow.Models;

namespace OrderFlow.Tests;

public class Tests
{
    [Test]
    public async Task PendingToConfirmed()
    {
        WebApp app = new();
        var db = app.GetService<MyContext>();
        var time = app.GetService<TimeProvider>();

        // create pending order
        Order.Pending pending = new()
        {
            CreatedAt = time.UtcNow(),
            Amount = 100,
            Owner = "Lucas",
        };
        db.Orders.Add(pending);
        await db.SaveChangesAsync();

        // Order confirmation, so it is "evolved"
        // to a Confirmed type 
        var confirmAt = time.UtcNow().AddDays(1);
        Order.Confirmed confirmed = pending.Confirm(confirmAt);
        db.Evolve(pending, confirmed); // <- swap entities
        await db.SaveChangesAsync();

        // Order finalization, so it is "evolved"
        var finalizedAt = time.UtcNow().AddDays(2);
        Order.Finalized finalized = confirmed.Finalize(confirmAt);
        db.Evolve(confirmed, finalized); // <- swap entities
        await db.SaveChangesAsync();
        
        await foreach (var order in db.Orders.AsNoTracking().AsAsyncEnumerable())
            order.Inspect();

        Console.WriteLine("------------------------------------");
        
        var res = await db.Query("SELECT * From \"Orders\"");
        res.Inspect();
        Assert.That(res[0]["Owner"], Is.EqualTo("Lucas"));
    }
}