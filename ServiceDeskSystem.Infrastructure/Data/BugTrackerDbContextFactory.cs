using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace ServiceDeskSystem.Infrastructure.Data
{
    public class BugTrackerDbContextFactory : IDesignTimeDbContextFactory<BugTrackerDbContext>
    {
        public BugTrackerDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                TryReadConnectionStringFromConfig()
                ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' was not found for EF Core design-time tools.");
            }

            var serverVersionValue = TryReadMySqlVersionFromConfig() ?? "8.0.45";
            if (!Version.TryParse(serverVersionValue, out var parsedVersion))
            {
                throw new InvalidOperationException("Configuration key 'Database:MySqlVersion' must be a valid version.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<BugTrackerDbContext>();
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(parsedVersion));

            return new BugTrackerDbContext(optionsBuilder.Options);
        }

        private static string? TryReadConnectionStringFromConfig()
        {
            foreach (var configPath in GetConfigurationFiles())
            {
                using var document = JsonDocument.Parse(
                    File.ReadAllText(configPath),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (document.RootElement.TryGetProperty("ConnectionStrings", out var connectionStrings)
                    && connectionStrings.TryGetProperty("DefaultConnection", out var connectionString))
                {
                    var value = connectionString.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return null;
        }

        private static string? TryReadMySqlVersionFromConfig()
        {
            foreach (var configPath in GetConfigurationFiles())
            {
                using var document = JsonDocument.Parse(
                    File.ReadAllText(configPath),
                    new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true });
                if (document.RootElement.TryGetProperty("Database", out var database)
                    && database.TryGetProperty("MySqlVersion", out var mySqlVersion))
                {
                    var value = mySqlVersion.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }
            }

            return null;
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

