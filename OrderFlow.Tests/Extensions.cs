using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

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

    public static async Task<IDictionary<string, object?>> Query(
        this DbContext context, string sqlCommand)
    {
        await using var command = context.Database.GetDbConnection().CreateCommand();
        command.CommandText = sqlCommand;
        await context.Database.OpenConnectionAsync();
        await using var reader = await command.ExecuteReaderAsync();
        Dictionary<string, object?> result = new();
        while (await reader.ReadAsync())
            for (var i = 0; i < reader.FieldCount; i++)
                result.Add(reader.GetName(i), reader.GetValue(i));
        return result;
    }
}