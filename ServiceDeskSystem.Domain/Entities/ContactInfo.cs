namespace ServiceDeskSystem.Domain.Entities;

public class ContactInfo
{
    public int Id { get; set; }

    public string Value { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public int PersonId { get; set; }

    public Person Person { get; set; } = null!;

    public int ContactTypeId { get; set; }

    public ContactType ContactType { get; set; } = null!;
}
