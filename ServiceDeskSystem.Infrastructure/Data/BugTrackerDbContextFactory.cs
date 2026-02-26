using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace ServiceDeskSystem.Infrastructure.Data
{
    public class BugTrackerDbContextFactory : IDesignTimeDbContextFactory<BugTrackerDbContext>
    {
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