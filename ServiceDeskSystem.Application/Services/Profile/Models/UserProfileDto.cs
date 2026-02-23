namespace ServiceDeskSystem.Application.Services.Profile.Models;

public sealed record UserProfileDto(
    int UserId,
    string Login,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? Bio,
    List<ContactInfoDto> Contacts
);

public sealed record ContactInfoDto(
    int Id,
    string Value,
    bool IsPrimary,
    int ContactTypeId,
    string ContactTypeName
);
