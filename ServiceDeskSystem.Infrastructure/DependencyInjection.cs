using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        return services;
    }
}
