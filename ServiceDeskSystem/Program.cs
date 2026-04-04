using ServiceDeskSystem.Application;
using ServiceDeskSystem.Components;
using ServiceDeskSystem.Infrastructure;
using ServiceDeskSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ServiceDeskSystem;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error", createScopeForErrors: true);
            app.UseHsts();
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseHttpsRedirection();

        app.UseAntiforgery();

        app.MapStaticAssets();
        app.MapGet("/health/db", async (IDbContextFactory<BugTrackerDbContext> dbContextFactory, CancellationToken cancellationToken) =>
        {
            try
            {
                await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
                var isAvailable = await dbContext.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
                return Results.Ok(new { IsAvailable = isAvailable });
            }
            catch
            {
                return Results.Ok(new { IsAvailable = false });
            }
        });

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync().ConfigureAwait(false);
    }
}
