namespace OrderFlow.Models;

[Serializable]
record NewOrder(decimal Amount, string Owner);