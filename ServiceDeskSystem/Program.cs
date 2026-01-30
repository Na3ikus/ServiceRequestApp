using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Components;
using ServiceDeskSystem.Data;
using ServiceDeskSystem.Services;

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
builder.Services.AddSingleton<IAuthService, SimpleAuthService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();
builder.Services.AddSingleton<IThemeService, ThemeService>();

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
