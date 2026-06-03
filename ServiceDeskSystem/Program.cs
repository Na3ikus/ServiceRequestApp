using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Components;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Hubs;
using ServiceDeskSystem.Infrastructure;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Services.Realtime;

namespace ServiceDeskSystem;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();
        builder.Services.AddHttpClient();
        builder.Services.AddSignalR();

        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddApplicationServices();
        builder.Services.AddSingleton<IRealtimeNotifier, SignalRRealtimeNotifier>();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbInitializer = scope.ServiceProvider.GetRequiredService<ServiceDeskSystem.Infrastructure.Data.DataSeeding.DbInitializer>();
            await dbInitializer.InitializeAsync();
        }

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

        app.MapGet("/health/smtp", async (IEmailSender emailSender, CancellationToken cancellationToken) =>
        {
            var (isSuccess, message) = await emailSender.CheckConnectionAsync(cancellationToken).ConfigureAwait(false);
            return Results.Ok(new { IsAvailable = isSuccess, Message = message });
        });
        app.MapHub<UpdatesHub>("/hubs/updates");

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync().ConfigureAwait(false);
    }
}
