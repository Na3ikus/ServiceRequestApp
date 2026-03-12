using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServiceDeskSystem.Infrastructure.Data
{
    public class BugTrackerDbContextFactory : IDesignTimeDbContextFactory<BugTrackerDbContext>
    {
        public BugTrackerDbContext CreateDbContext(string[] args)
        {
            var connectionString =
                Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? "Server=localhost;Database=BugTrackerDB;User=root;Password=changeme;";

            var optionsBuilder = new DbContextOptionsBuilder<BugTrackerDbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new BugTrackerDbContext(optionsBuilder.Options);
        }
    }
}

