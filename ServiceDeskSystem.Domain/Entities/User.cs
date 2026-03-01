namespace ServiceDeskSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int PersonId { get; set; }

    [System.Text.Json.Serialization.JsonIgnore]
    public Person Person { get; set; } = null!;

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    [System.Text.Json.Serialization.JsonIgnore]
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
