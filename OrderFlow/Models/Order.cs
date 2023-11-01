namespace OrderFlow.Models;

public abstract class Order(Order.StatusEnum status)
{
    public OrderId Id { get; init; } = OrderId.New();
    public required decimal Amount { get; init; }
    public required DateTime CreatedAt { get; init; }

    public StatusEnum Status { get; private set; } = status;

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

        public Confirmed Confirm(DateTime date) =>
            new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                ConfirmedAt = date,
            };

        public Cancelled Cancel(DateTime date) =>
            new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                CancelledAt = date,
            };
    }

    public class Confirmed() : Order(StatusEnum.Confirmed), ICancellable
    {
        public required DateTime ConfirmedAt { get; init; }

        public Finalized Finalize(DateTime date) =>
            new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                FinalizedAt = date,
            };

        public Cancelled Cancel(DateTime date) =>
            new()
            {
                Id = Id,
                Amount = Amount,
                CreatedAt = CreatedAt,
                CancelledAt = date,
            };
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