using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Email;
using ServiceDeskSystem.Infrastructure.Data;
using ServiceDeskSystem.Infrastructure.Data.Repository;

namespace ServiceDeskSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "server=127.0.0.1;user=root;password=;database=BugTrackerDB";
            Console.WriteLine("WARNING: Connection string 'DefaultConnection' not found. Using fallback value. Please configure appsettings.json.");
        }

        var serverVersionValue = configuration["Database:MySqlVersion"] ?? "8.0.45";
        if (!Version.TryParse(serverVersionValue, out var parsedVersion))
        {
            parsedVersion = new Version(8, 0, 45);
            Console.WriteLine($"WARNING: Configuration key 'Database:MySqlVersion' has invalid value '{serverVersionValue}'. Falling back to 8.0.45.");
        }

        var serverVersion = new MySqlServerVersion(parsedVersion);

        services.AddDbContextFactory<BugTrackerDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                mySqlOptions.EnableRetryOnFailure()));

        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<BugTrackerDbContext>>().CreateDbContext());

        var smtpSection = configuration.GetSection(SmtpOptions.SectionName);
        if (!smtpSection.Exists() || !smtpSection.GetChildren().Any())
        {
            Console.WriteLine("WARNING: SMTP configuration section not found. Email functionality will be disabled.");
            services.Configure<SmtpOptions>(o => o.Enabled = false);
        }
        else
        {
            services.AddOptions<SmtpOptions>()
                .Bind(smtpSection)
                .Validate(options =>
                    !options.Enabled ||
                    (!string.IsNullOrWhiteSpace(options.Host)
                     && !string.IsNullOrWhiteSpace(options.FromEmail)
                     && (!options.UseAuthentication ||
                         (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password)))),
                    "When SMTP is enabled, Host/FromEmail and authentication credentials (if enabled) must be configured.");
        }

        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IUnitOfWork, ServiceDeskSystem.Infrastructure.Data.UnitOfWork>();
        services.AddScoped<IRepositoryFacadeFactory, RepositoryFacadeFactory>();
        services.AddScoped<ServiceDeskSystem.Infrastructure.Data.DataSeeding.DbInitializer>();

        return services;
    }
}
