using System.Net;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Components;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Services.Presence;

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
        builder.Services.AddHttpContextAccessor();

        // One instance: services raise changes through IRealtimeNotifier, open pages listen through IRealtimeEvents.
        builder.Services.AddSingleton<InProcessRealtimeNotifier>();
        builder.Services.AddSingleton<IRealtimeNotifier>(sp => sp.GetRequiredService<InProcessRealtimeNotifier>());
        builder.Services.AddSingleton<IRealtimeEvents>(sp => sp.GetRequiredService<InProcessRealtimeNotifier>());

        // Who is connected (admin "online" tab): one tracker, one handler per circuit that pages can ask for its id.
        builder.Services.AddSingleton<OnlineSessionTracker>();
        builder.Services.AddScoped<OnlineCircuitHandler>();
        builder.Services.AddScoped<CircuitHandler>(sp => sp.GetRequiredService<OnlineCircuitHandler>());

        // Behind a reverse proxy (nginx, IIS) the client's address and scheme come in X-Forwarded-* headers.
        // Only proxies on this machine are trusted by default; list others in ForwardedHeaders:KnownProxies.
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            foreach (var proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });

        var app = builder.Build();

        app.UseForwardedHeaders();

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

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync().ConfigureAwait(false);
    }
}
