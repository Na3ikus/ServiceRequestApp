using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace ServiceDeskSystem.Data
{
    public class BugTrackerDbContext : DbContext
    {
        private readonly string? customConnectionString;

        public BugTrackerDbContext()
        {
        }

        public BugTrackerDbContext(string? customConnectionString)
        {
            this.customConnectionString = customConnectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            ArgumentNullException.ThrowIfNull(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                if (!string.IsNullOrEmpty(this.customConnectionString))
                {
                    optionsBuilder.UseMySql(this.customConnectionString, ServerVersion.AutoDetect(this.customConnectionString));
                    return;
                }

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("app_config.json", optional: false, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();

                string connectionString = configuration.GetConnectionString("DefaultConnection")
                    ?? throw new InvalidOperationException("Рядок підключення 'DefaultConnection' не знайдено у файлі app_config.json.");
                optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
            }
        }
    }
}
