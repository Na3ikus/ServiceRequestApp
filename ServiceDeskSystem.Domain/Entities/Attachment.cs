namespace ServiceDeskSystem.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string StoredFileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int? UploadedById { get; set; }

    public User? UploadedBy { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public int? CommentId { get; set; }

    public Comment? Comment { get; set; }
}
