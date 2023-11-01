using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderFlow.Models;

public record OrderHistory(
    DateTime Date,
    Order.StatusEnum Status,
    [property: JsonIgnore] JsonElement Photo
);