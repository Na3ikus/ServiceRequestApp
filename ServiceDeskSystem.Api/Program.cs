using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using ServiceDeskSystem.Api.Middleware;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Api;

public static class Program
{
    public static async Task Main(string[] args)
    {
        // ───────── Serilog Bootstrap ─────────
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("Starting ServiceDeskSystem API");

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilog();

            // ───────── Database ─────────
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContextFactory<BugTrackerDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

            builder.Services.AddScoped(sp =>
                sp.GetRequiredService<IDbContextFactory<BugTrackerDbContext>>().CreateDbContext());

            // ───────── Services DI ─────────
            builder.Services.AddScoped<ITicketService, TicketService>();
            builder.Services.AddScoped<IAuthService, SimpleAuthService>();
            builder.Services.AddScoped<IAdminService, AdminService>();

            // ───────── Swagger ─────────
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ServiceDesk API",
                    Version = "v1",
                    Description = "REST API for the Service Desk System",
                });
            });

            // ───────── Controllers & CORS ─────────
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
                });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // ───────── Build & Configure Pipeline ─────────
            var app = builder.Build();

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceDesk API v1");
                    c.RoutePrefix = "swagger";
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseSerilogRequestLogging();

            // TODO: JWT Authentication буде додано пізніше
            // app.UseAuthentication();
            // app.UseAuthorization();

            app.MapControllers();

            await app.RunAsync().ConfigureAwait(false);
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
