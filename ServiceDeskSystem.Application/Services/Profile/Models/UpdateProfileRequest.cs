namespace ServiceDeskSystem.Application.Services.Profile.Models;

public sealed class UpdateProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string? Bio { get; set; }
    public List<UpdateContactRequest> Contacts { get; set; } = new();
}

public sealed class UpdateContactRequest
{
    public int? Id { get; set; } // If null -> new contact
    public string Value { get; set; } = string.Empty;
    public int ContactTypeId { get; set; }
    public bool IsPrimary { get; set; }
}
