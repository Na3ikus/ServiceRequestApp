using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding;

internal sealed class TechStackConfiguration : IEntityTypeConfiguration<TechStack>
{
    public void Configure(EntityTypeBuilder<TechStack> builder)
    {
        builder.HasData(
            new TechStack { Id = 1, Name = "C# / .NET", Type = "Desktop Software" },
            new TechStack { Id = 2, Name = "ASP.NET Core / Blazor", Type = "Web Application" },
            new TechStack { Id = 3, Name = "Android / Kotlin", Type = "Mobile Application" },
            new TechStack { Id = 4, Name = "C++ / Embedded", Type = "Hardware Firmware" },
            new TechStack { Id = 5, Name = "Python / Django", Type = "Web Service" },
            new TechStack { Id = 6, Name = "Network Infrastructure", Type = "Hardware" });
    }
}
