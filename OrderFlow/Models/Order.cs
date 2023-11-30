using System.Text.Json;

namespace OrderFlow.Models;

public abstract class Order(Order.StatusEnum status)
{
    public OrderId Id { get; init; } = OrderId.New();
    public StatusEnum Status { get; private init; } = status;
    public required decimal Amount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; private set; }

    readonly List<OrderHistory> history = new();

    public IReadOnlyList<OrderHistory> History => history.AsReadOnly();

    public void Updated(DateTime date)
    {
        UpdatedAt = date;
        history.Add(new(date, Status, JsonSerializer.SerializeToElement(this)));
    }

    public enum StatusEnum
    {
        Pending,
        Confirmed,
        Cancelled,
        Finalized,
    }

    public interface ICancellable
    {
        Cancelled Cancel(DateTime date);
    }

    public class Pending() : Order(StatusEnum.Pending), ICancellable
    {
        public required string Owner { get; init; }
        
        public Confirmed Confirm(DateTime date)
        {
            Confirmed next = new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                ConfirmedAt = date,
            };

            next.Updated(date);
            return next;
        }

        public Cancelled Cancel(DateTime date)
        {
            Cancelled next = new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                CancelledAt = date,
            };

            next.Updated(date);
            return next;
        }
    }

    public class Confirmed() : Order(StatusEnum.Confirmed), ICancellable
    {
        public required DateTime ConfirmedAt { get; init; }

        public Finalized Finalize(DateTime date)
        {
            Finalized next = new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                FinalizedAt = date,
            };
            next.Updated(date);
            return next;
        }

        public Cancelled Cancel(DateTime date)
        {
            Cancelled next = new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                CancelledAt = date,
            };
            next.Updated(date);
            return next;
        }
    }

    public class Cancelled() : Order(StatusEnum.Cancelled)
    {
        public required DateTime CancelledAt { get; init; }
    }

    public class Finalized() : Order(StatusEnum.Finalized)
    {
        public required DateTime FinalizedAt { get; init; }
    }
}