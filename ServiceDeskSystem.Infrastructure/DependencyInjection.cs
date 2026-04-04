using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Email;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        var serverVersionValue = configuration["Database:MySqlVersion"] ?? "8.0.45";
        if (!Version.TryParse(serverVersionValue, out var parsedVersion))
        {
            throw new InvalidOperationException("Configuration key 'Database:MySqlVersion' must be a valid version (for example: 8.0.36).");
        }

        var serverVersion = new MySqlServerVersion(parsedVersion);

        services.AddDbContextFactory<BugTrackerDbContext>(options =>
            options.UseMySql(connectionString, serverVersion, mySqlOptions =>
                mySqlOptions.EnableRetryOnFailure()));

        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<BugTrackerDbContext>>().CreateDbContext());

        services.AddOptions<SmtpOptions>()
            .Bind(configuration.GetSection(SmtpOptions.SectionName))
            .Validate(options =>
                !options.Enabled ||
                (!string.IsNullOrWhiteSpace(options.Host)
                 && !string.IsNullOrWhiteSpace(options.FromEmail)
                 && (!options.UseAuthentication ||
                     (!string.IsNullOrWhiteSpace(options.Username) && !string.IsNullOrWhiteSpace(options.Password)))),
                "When SMTP is enabled, Host/FromEmail and authentication credentials (if enabled) must be configured.")
            .ValidateOnStart();

        services.AddScoped<IEmailSender, SmtpEmailSender>();

        return services;
    }
}
