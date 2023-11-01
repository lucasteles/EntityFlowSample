using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using OrderFlow;
using OrderFlow.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .ConfigureHttpJsonOptions(opt => opt.SerializerOptions
        .Converters.Add(new JsonStringEnumConverter()))
    .AddSingleton(TimeProvider.System)
    .AddScoped<OrderRepository>()
    .AddDbContext<MyContext>(opt => opt
        .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

app.UseSwagger().UseSwaggerUI();
app.MapRoutes();

await using var scope = app.Services.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<MyContext>();
await db.Database.EnsureDeletedAsync();
await db.Database.EnsureCreatedAsync();

await app.RunAsync();