using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding.Tables;

internal sealed class TicketConfiguration : IEntityTypeConfiguration<Ticket>
{
    public void Configure(EntityTypeBuilder<Ticket> builder)
    {
        var priorities = new[] { "Critical", "High", "Medium", "Low" };
        var statuses = new[] { "New", "In Progress", "Code Review", "Testing", "Done" };

        var ticketFaker = new Faker<Ticket>()
            .UseSeed(123)
            .RuleFor(t => t.Id, f => f.IndexFaker + 1)
            .RuleFor(t => t.Title, f => f.Lorem.Sentence(5).TrimEnd('.'))
            .RuleFor(t => t.Description, f => f.Lorem.Paragraph(3))
            .RuleFor(t => t.StepsToReproduce, f => string.Join("\n", f.Lorem.Sentences(3)))
            .RuleFor(t => t.Priority, f => f.PickRandom(priorities))
            .RuleFor(t => t.Status, f => f.PickRandom(statuses))
            .RuleFor(t => t.AffectedVersion, f => f.System.Version().ToString())
            .RuleFor(t => t.Environment, f => $"{f.PickRandom("Windows 10", "Windows 11", "Ubuntu 22.04", "macOS Ventura")} / {f.PickRandom("Chrome", "Firefox", "Edge", "Safari")}")
            .RuleFor(t => t.CreatedAt, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now))
            .RuleFor(t => t.ProductId, f => f.Random.Int(1, 5))
            .RuleFor(t => t.AuthorId, f => 3)
            .RuleFor(t => t.DeveloperId, f => f.Random.Bool(0.7f) ? 2 : (int?)null);

        var tickets = ticketFaker.Generate(20);

        builder.HasData(tickets);
    }
}
