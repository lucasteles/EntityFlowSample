using System.Text.Json;
using System.Text.Json.Serialization;

namespace OrderFlow.Tests;

public static class Extensions
{
    public static void Inspect(this object value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = {new JsonStringEnumConverter()},
            });
            Console.WriteLine(json);
        }
        catch
        {
            Console.WriteLine(value.ToString());
        }
    }

    public static DateTime UtcNow(this TimeProvider time) =>
        time.GetUtcNow().UtcDateTime;
}