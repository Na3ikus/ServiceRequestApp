namespace ServiceDeskSystem.Domain.Entities;

public class Person
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? MiddleName { get; set; }

    public User? User { get; set; }

    public ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
}
