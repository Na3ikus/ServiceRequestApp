namespace ServiceDeskSystem.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }

    public string Action { get; set; } = string.Empty;

    public string EntityName { get; set; } = string.Empty;

    public string EntityId { get; set; } = string.Empty;

    public string? Changes { get; set; }

    public DateTime Timestamp { get; set; }

    public int? UserId { get; set; }

    public User? User { get; set; }
}
