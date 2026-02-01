namespace ServiceDeskSystem.Data.Entities;

internal class User
{
    public int Id { get; set; }

    public string Login { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public ICollection<Ticket> CreatedTickets { get; set; } = new List<Ticket>();

    public ICollection<Ticket> AssignedTickets { get; set; } = new List<Ticket>();

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
