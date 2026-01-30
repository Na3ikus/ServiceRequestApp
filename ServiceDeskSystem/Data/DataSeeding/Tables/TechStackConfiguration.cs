using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding.Tables;

internal class TechStackConfiguration : IEntityTypeConfiguration<TechStack>
{
    public void Configure(EntityTypeBuilder<TechStack> builder)
    {
        builder.HasData(
            new TechStack { Id = 1, Name = "C# / .NET", Type = "Backend" },
            new TechStack { Id = 2, Name = "Blazor", Type = "Web" },
            new TechStack { Id = 3, Name = "React", Type = "Web" },
            new TechStack { Id = 4, Name = "SQL Server", Type = "Database" },
            new TechStack { Id = 5, Name = "C++ STM32", Type = "Embedded" }
        );
    }
}
