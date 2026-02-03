using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal static class TechStackConfiguration
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TechStack>().HasData(
            new TechStack { Id = 1, Name = "C# / .NET", Type = "Desktop Software" },
            new TechStack { Id = 2, Name = "ASP.NET Core / Blazor", Type = "Web Application" },
            new TechStack { Id = 3, Name = "Android / Kotlin", Type = "Mobile Application" },
            new TechStack { Id = 4, Name = "C++ / Embedded", Type = "Hardware Firmware" },
            new TechStack { Id = 5, Name = "Python / Django", Type = "Web Service" },
            new TechStack { Id = 6, Name = "Network Infrastructure", Type = "Hardware" });
    }
}
