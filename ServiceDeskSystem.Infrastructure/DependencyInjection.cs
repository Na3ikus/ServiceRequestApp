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
        var (connectionString, serverVersion) = DatabaseConfigurationHelper.GetDatabaseConfiguration(configuration);

        services.AddPooledDbContextFactory<BugTrackerDbContext>(options =>
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
