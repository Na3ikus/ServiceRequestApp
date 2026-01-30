using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Pomelo.EntityFrameworkCore.MySql;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data
{
    internal class BugTrackerDbContext : DbContext
    {
        public BugTrackerDbContext(DbContextOptions<BugTrackerDbContext> options)
                    : base(options)
        {
        }

        public DbSet<Person> People { get; set; }

        public DbSet<ContactType> ContactTypes { get; set; }

        public DbSet<ContactInfo> ContactInfos { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<TechStack> TechStacks { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Attachment> Attachments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasOne(u => u.Person)
                .WithOne(p => p.User)
                .HasForeignKey<User>(u => u.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Login)
                .IsUnique();

            modelBuilder.Entity<ContactInfo>()
                .HasOne(ci => ci.Person)
                .WithMany(p => p.ContactInfos)
                .HasForeignKey(ci => ci.PersonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ContactInfo>()
                .HasOne(ci => ci.ContactType)
                .WithMany(ct => ct.ContactInfos)
                .HasForeignKey(ci => ci.ContactTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasOne(p => p.TechStack)
                .WithMany(ts => ts.Products)
                .HasForeignKey(p => p.TechStackId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Product)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Author)
                .WithMany(u => u.CreatedTickets)
                .HasForeignKey(t => t.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Developer)
                .WithMany(u => u.AssignedTickets)
                .HasForeignKey(t => t.DeveloperId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Author)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attachment>()
                .HasOne(a => a.Ticket)
                .WithMany(t => t.Attachments)
                .HasForeignKey(a => a.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(BugTrackerDbContext).Assembly);
        }
    }
}