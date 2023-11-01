using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace OrderFlow.Tests;

sealed class WebApp : WebApplicationFactory<Program>
{
    public AsyncServiceScope Scope { get; }
    public HttpClient Client => CreateClient();

    public WebApp() => Scope = Services.CreateAsyncScope();

    public T GetService<T>() where T : notnull =>
        Scope.ServiceProvider.GetRequiredService<T>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var dir = Directory.GetCurrentDirectory();
        builder.UseContentRoot(dir);
    }

    public override async ValueTask DisposeAsync()
    {
        await Scope.DisposeAsync();
        await base.DisposeAsync();
    }
}