namespace ServiceDeskSystem.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string StepsToReproduce { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string AffectedVersion { get; set; } = string.Empty;

    public string Environment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public int ProductId { get; set; }

    public Product Product { get; set; } = null!;

    public int AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public int? DeveloperId { get; set; }

    public User? Developer { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
}
