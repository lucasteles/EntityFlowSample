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

        // Order is confirmed, so it is "evolved"
        // to a Confirmed type 
        Order.Confirmed confirmed = pending.Confirm(time.UtcNow().AddDays(1));
        db.Evolve(pending, confirmed); // <- swap entities
        await db.SaveChangesAsync();

        await foreach (var order in db.Orders)
            order.Inspect();

        var res = await db.Query("SELECT * From \"Orders\"");
        res.Inspect();
        Assert.That(res["Owner"], Is.EqualTo("Lucas"));
    }
}