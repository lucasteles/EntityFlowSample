using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderFlow.Models;

[JsonConverter(typeof(OrderIdJsonConverter))]
public readonly record struct OrderId(Guid Value)
{
    public static OrderId New() => new(Guid.NewGuid());

    public static bool TryParse(string? value, IFormatProvider? provider, out OrderId id)
    {
        if (Guid.TryParse(value, provider, out var guid))
        {
            id = new(guid);
            return true;
        }

        id = default;
        return false;
    }

    public class OrderIdJsonConverter : JsonConverter<OrderId>
    {
        public override OrderId Read(ref Utf8JsonReader reader,
            Type typeToConvert, JsonSerializerOptions options
        ) => new(reader.GetGuid());

        public override void Write(Utf8JsonWriter writer,
            OrderId value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value.Value);
    }
}