using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ServiceDeskSystem.Infrastructure.Data;

public static class DatabaseConfigurationHelper
{
    public static (string ConnectionString, MySqlServerVersion ServerVersion) GetDatabaseConfiguration(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? configuration["ConnectionStrings:DefaultConnection"];

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

        return (connectionString, new MySqlServerVersion(parsedVersion));
    }
}
