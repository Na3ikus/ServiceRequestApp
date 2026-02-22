using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding
{
    internal interface IDataSeed
    {
        DbSet<Person> People { get; }

        DbSet<ContactType> ContactTypes { get; }

        DbSet<ContactInfo> ContactInfos { get; }

        DbSet<User> Users { get; }

        DbSet<TechStack> TechStacks { get; }

        DbSet<Product> Products { get; }

        DbSet<Ticket> Tickets { get; }

        DbSet<Comment> Comments { get; }

        DbSet<Attachment> Attachments { get; }
    }
}
