using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OrderFlow;
using OrderFlow.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .ConfigureHttpJsonOptions(opt => opt.SerializerOptions
        .Converters.Add(new JsonStringEnumConverter()))
    .AddSingleton(TimeProvider.System)
    .AddDbContext<MyContext>(opt => opt
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")))
    .AddEndpointsApiExplorer()
    .AddSwaggerGen(opt =>
    {
        opt.MapType<OrderId>(() => new OpenApiSchema {Type = "string", Format = "uuid"});
    });

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();
app.MapRoutes();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<MyContext>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

await app.RunAsync();