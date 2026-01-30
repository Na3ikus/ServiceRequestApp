using Bogus;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ServiceDeskSystem.Data.Entities;

namespace ServiceDeskSystem.Data.DataSeeding.Tables;

internal sealed class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        Randomizer.Seed = new Random(123);

        var commentFaker = new Faker<Comment>()
            .RuleFor(c => c.Id, f => f.IndexFaker + 1)
            .RuleFor(c => c.Message, f => f.Lorem.Paragraph(2))
            .RuleFor(c => c.IsInternal, f => f.Random.Bool(0.3f))
            .RuleFor(c => c.CreatedAt, f => f.Date.Between(DateTime.Now.AddMonths(-6), DateTime.Now))
            .RuleFor(c => c.TicketId, f => f.Random.Int(1, 20))
            .RuleFor(c => c.AuthorId, f => f.PickRandom(2, 3));

        var comments = commentFaker.Generate(50);

        builder.HasData(comments);
    }
}
