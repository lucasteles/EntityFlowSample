using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OrderFlow.Models;

namespace OrderFlow;

using static TypedResults;

public static class Routes
{
    public static void MapRoutes(this IEndpointRouteBuilder app)
    {
        var route = app.MapGroup("orders");
        route.MapGet("/", (MyContext db) => db.Orders.ToListAsync());
        route.MapGet("/{id}",
            async Task<Results<Ok<Order>, NotFound>>
                (MyContext db, OrderId id) =>
                await db.Orders.FindAsync(id) is not { } order
                    ? NotFound()
                    : Ok(order)
        );

        route.MapPost("/",
            async Task<Results<Created<Order>, BadRequest>>
                (MyContext db, TimeProvider time, NewOrder newOrder) =>
            {
                if (newOrder is not {Amount: > 0, Owner: not null and not ""})
                    return BadRequest();

                Order.Pending order = new()
                {
                    CreatedAt = time.UtcNow(),
                    Amount = newOrder.Amount,
                    Owner = newOrder.Owner,
                };
                order.TakePhoto(time.UtcNow());
                db.Orders.Add(order);
                await db.SaveChangesAsync();
                return Created($"orders/{order.Id}", order as Order);
            });

        route.MapPost("/{id}/confirm",
            async Task<Results<NotFound, NoContent, UnprocessableEntity>>
                (MyContext db, TimeProvider time, OrderId id) =>
            {
                if (await db.Orders.FindById(id) is not { } order)
                    return NotFound();

                if (order is not Order.Pending pending)
                    return UnprocessableEntity();

                var confirmed = pending.Confirm(time.UtcNow());

                db.Evolve(pending, confirmed);
                await db.SaveChangesAsync();

                return NoContent();
            });

        route.MapPost("/{id}/cancel",
            async Task<Results<NotFound, NoContent, UnprocessableEntity>>
                (MyContext db, TimeProvider time, OrderId id) =>
            {
                if (await db.Orders.FirstOrDefaultAsync(x => x.Id == id)
                    is not { } order)
                    return NotFound();

                if (order is not Order.ICancellable cancellable)
                    return UnprocessableEntity();

                var cancelled = cancellable.Cancel(time.UtcNow());

                db.Evolve(order, cancelled);
                await db.SaveChangesAsync();

                return NoContent();
            });

        route.MapPost("/{id}/finalize",
            async Task<Results<NotFound, NoContent, UnprocessableEntity>>
                (MyContext db, TimeProvider time, OrderId id) =>
            {
                if (await db.Orders.FindById(id) is not { } order)
                    return NotFound();

                if (order is not Order.Confirmed confirmed)
                    return UnprocessableEntity();

                var finalized = confirmed.Finalize(time.UtcNow());

                db.Evolve(confirmed, finalized);
                await db.SaveChangesAsync();

                return NoContent();
            });
    }
}