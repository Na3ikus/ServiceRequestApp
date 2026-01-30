using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding.Tables;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasData(
            new User
            {
                Id = 1,
                Login = "admin",
                PasswordHash = "$2a$11$dummyHashForTestingPurposesOnly123456",
                Role = "Admin",
                PersonId = 1
            },
            new User
            {
                Id = 2,
                Login = "developer",
                PasswordHash = "$2a$11$dummyHashForTestingPurposesOnly123456",
                Role = "Developer",
                PersonId = 2
            },
            new User
            {
                Id = 3,
                Login = "client",
                PasswordHash = "$2a$11$dummyHashForTestingPurposesOnly123456",
                Role = "Client",
                PersonId = 3
            }
        );
    }
}
