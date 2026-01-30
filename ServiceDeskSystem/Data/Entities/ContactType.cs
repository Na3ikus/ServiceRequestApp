namespace ServiceDeskSystem.Data.Entities;

public class ContactType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public ICollection<ContactInfo> ContactInfos { get; set; } = new List<ContactInfo>();
}
