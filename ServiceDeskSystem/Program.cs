using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Admin.Interfaces;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Comments.Interfaces;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Theme;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;
using ServiceDeskSystem.Components;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContextFactory<BugTrackerDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        builder.Services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<BugTrackerDbContext>>().CreateDbContext());

        builder.Services.AddScoped<ITicketService, TicketService>();
        builder.Services.AddScoped<ITicketAssignmentService, TicketService>();
        builder.Services.AddScoped<ITicketStatisticsService, TicketService>();
        builder.Services.AddScoped<ICommentService, CommentService>();
        builder.Services.AddScoped<IAuthService, SimpleAuthService>();
        builder.Services.AddScoped<ILocalizationService, LocalizationService>();
        builder.Services.AddScoped<IThemeService, ThemeService>();
        builder.Services.AddScoped<IAdminService, AdminService>();
        builder.Services.AddScoped<ServiceDeskSystem.Application.Services.Profile.Interfaces.IProfileService, ServiceDeskSystem.Application.Services.Profile.ProfileService>();

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
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        await app.RunAsync().ConfigureAwait(false);
    }
}
