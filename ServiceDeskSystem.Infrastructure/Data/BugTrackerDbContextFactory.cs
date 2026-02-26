using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ServiceDeskSystem.Infrastructure.Data
{
    public class BugTrackerDbContextFactory : IDesignTimeDbContextFactory<BugTrackerDbContext>
    {
        // Rework the test connection, into a connection from a file.
        public BugTrackerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BugTrackerDbContext>();
            optionsBuilder.UseMySql(
                "Server=localhost;Database=BugTrackerDB;User=root;Password=123456789;",
                ServerVersion.AutoDetect("Server=127.0.0.1;Database=BugTrackerDB;User=root;Password=123456789;")
            );

            return new BugTrackerDbContext(optionsBuilder.Options);
        }
    }
}
