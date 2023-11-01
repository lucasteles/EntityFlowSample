using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Data;
using OrderFlow.Models;

namespace OrderFlow;

using static TypedResults;

public static class Routes
{
    public static void MapRoutes(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("orders");
        orders.MapGet("/", (OrderRepository db) => db.Orders.ToListAsync());
        orders.MapGet("/{id}",
            async Task<Results<Ok<Order>, NotFound>>
                (OrderRepository db, OrderId id) =>
                await db.Orders.FindAsync(id) is not { } order
                    ? NotFound()
                    : Ok(order)
        );

        orders.MapPost("/",
            async Task<Results<Created, BadRequest>>
                (OrderRepository db, TimeProvider time, NewOrder newOrder) =>
            {
                if (newOrder is not {Amount: > 0, Owner: not null and not ""})
                    return BadRequest();

                Order.Pending order = new()
                {
                    CreatedAt = time.GetUtcNow().UtcDateTime,
                    Amount = newOrder.Amount,
                    Owner = newOrder.Owner,
                };
                db.Orders.Add(order);
                await db.Save();
                return Created();
            });

        orders.MapPost("/{id}/confirm",
            async Task<Results<NotFound, NoContent, UnprocessableEntity>>
                (OrderRepository db, TimeProvider time, OrderId id) =>
            {
                if (await db.Pending.FirstOrDefaultAsync(x => x.Id == id) is not { } pending)
                    return NotFound();

                var confirmed = pending.Confirm(time.GetUtcNow().UtcDateTime);

                db.Evolve(pending, confirmed);
                await db.Save();

                return NoContent();
            });

        orders.MapPost("/{id}/cancel",
            async Task<Results<NotFound, NoContent, UnprocessableEntity>>
                (OrderRepository db, TimeProvider time, OrderId id) =>
            {
                if (await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
                    is not { } order)
                    return NotFound();

                if (order is not Order.ICancellable cancellable)
                    return UnprocessableEntity();

                var cancelled = cancellable.Cancel(time.GetUtcNow().UtcDateTime);

                db.Evolve(order, cancelled);
                await db.Save();

                return NoContent();
            });
        
        orders.MapPost("/{id}/finalize",
            async Task<Results<NotFound, NoContent>>
                (OrderRepository db, TimeProvider time, OrderId id) =>
            {
                if (await db.Pending.FirstOrDefaultAsync(x => x.Id == id) is not { } pending)
                    return NotFound();

                var confirmed = pending.Confirm(time.GetUtcNow().UtcDateTime);

                db.Evolve(pending, confirmed);
                await db.Save();

                return NoContent();
            });
        
    }

    record NewOrder(decimal Amount, string Owner);
}