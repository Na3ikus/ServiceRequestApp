namespace ServiceDeskSystem.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;
}
