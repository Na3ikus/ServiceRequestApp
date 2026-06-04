using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace ServiceDeskSystem.Infrastructure.Data
{
    public class BugTrackerDbContextFactory : IDesignTimeDbContextFactory<BugTrackerDbContext>
    {
        public BugTrackerDbContext CreateDbContext(string[] args)
        {
            var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
                .AddEnvironmentVariables()
                .SetBasePath(Directory.GetCurrentDirectory());

            foreach (var configPath in GetConfigurationFiles())
            {
                configuration.AddJsonFile(configPath, optional: true);
            }

            var config = configuration.Build();

            var (connectionString, serverVersion) = DatabaseConfigurationHelper.GetDatabaseConfiguration(config);

            var optionsBuilder = new DbContextOptionsBuilder<BugTrackerDbContext>();
            optionsBuilder.UseMySql(connectionString, serverVersion);

            return new BugTrackerDbContext(optionsBuilder.Options);
        }

        private static IEnumerable<string> GetConfigurationFiles()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var candidateDirectories = new[]
            {
                currentDirectory,
                Path.Combine(currentDirectory, "ServiceDeskSystem"),
                Path.Combine(currentDirectory, "ServiceDeskSystem.Api"),
                AppContext.BaseDirectory,
            }
            .Where(Directory.Exists)
            .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var directory in candidateDirectories)
            {
                var appsettings = Path.Combine(directory, "appsettings.json");
                if (File.Exists(appsettings))
                {
                    yield return appsettings;
                }

                var envAppsettings = Path.Combine(directory, $"appsettings.{environment}.json");
                if (File.Exists(envAppsettings))
                {
                    yield return envAppsettings;
                }
            }
        }
    }
}

