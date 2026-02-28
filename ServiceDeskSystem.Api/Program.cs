using Serilog;
using ServiceDeskSystem.Api.Extensions;
using ServiceDeskSystem.Application;
using ServiceDeskSystem.Infrastructure;

namespace ServiceDeskSystem.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        try
        {
            Log.Information("Starting ServiceDeskSystem API");

            // ───────── Builder ─────────
            var builder = WebApplication.CreateBuilder(args);
            builder.AddSerilogLogging();

            // ───────── DI ─────────
            builder.Services.AddInfrastructureServices(builder.Configuration);
            builder.Services.AddCoreServices();
            builder.Services.AddApiConfiguration();

            // ───────── Pipeline ─────────
            var app = builder.Build();
            app.ConfigurePipeline();

            await app.RunAsync().ConfigureAwait(false);
        }
        catch (HostAbortedException)
        {
            // This exception is thrown normally by EF Core tools.
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            await Log.CloseAndFlushAsync().ConfigureAwait(false);
        }
    }
}
