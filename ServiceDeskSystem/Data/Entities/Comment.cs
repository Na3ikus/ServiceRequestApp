namespace ServiceDeskSystem.Data.Entities;

public class Comment
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;

    public bool IsInternal { get; set; }

    public DateTime CreatedAt { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public int AuthorId { get; set; }

    public User Author { get; set; } = null!;
}
