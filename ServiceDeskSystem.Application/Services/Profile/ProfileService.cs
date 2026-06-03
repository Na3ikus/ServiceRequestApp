using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Audit;

namespace ServiceDeskSystem.Application.Services.Profile;

public sealed class ProfileService(IRepositoryFacadeFactory repositoryFacadeFactory, IAuditService? auditService = null) : IProfileService
{

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var user = await repo.Users.GetByIdWithPersonAndContactsAsync(userId).ConfigureAwait(false);

        if (user?.Person == null)
            return null;

        var contacts = user.Person.ContactInfos.Select(ci => new ContactInfoDto(
            ci.Id,
            ci.Value,
            ci.IsPrimary,
            ci.ContactTypeId,
            ci.ContactType.Name
        )).ToList();

        return new UserProfileDto(
            user.Id,
            user.Login,
            user.Person.FirstName,
            user.Person.LastName,
            user.Person.MiddleName,
            user.Person.Bio,
            contacts
        );
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var user = await repo.Users.GetByIdWithPersonAndContactsAsync(userId).ConfigureAwait(false);

        if (user?.Person == null)
            return (false, "User not found.");

        user.Person.FirstName = request.FirstName;
        user.Person.LastName = request.LastName;
        user.Person.MiddleName = request.MiddleName;
        user.Person.Bio = request.Bio;

        var existingContacts = user.Person.ContactInfos.ToList();

        var requestContactIds = request.Contacts.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();
        var toRemove = existingContacts.Where(c => !requestContactIds.Contains(c.Id)).ToList();
        foreach (var r in toRemove)
        {
            await repo.ContactInfos.DeleteAsync(r.Id).ConfigureAwait(false);
        }

        foreach (var reqContact in request.Contacts)
        {
            if (reqContact.Id.HasValue && reqContact.Id.Value > 0)
            {
                var existing = existingContacts.FirstOrDefault(c => c.Id == reqContact.Id.Value);
                if (existing != null)
                {
                    existing.Value = reqContact.Value;
                    existing.ContactTypeId = reqContact.ContactTypeId;
                    existing.IsPrimary = reqContact.IsPrimary;
                }
            }
            else
            {
                var newContact = new ContactInfo
                {
                    Value = reqContact.Value,
                    ContactTypeId = reqContact.ContactTypeId,
                    IsPrimary = reqContact.IsPrimary,
                    PersonId = user.PersonId
                };
                await repo.ContactInfos.CreateAsync(newContact).ConfigureAwait(false);
            }
        }

        try
        {
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            if (auditService is not null)
            {
                await auditService.LogActionSafeAsync("UPDATE_PROFILE", "User", userId.ToString(), $"Updated user profile: {user.Login}", userId).ConfigureAwait(false);
            }
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to update profile: {ex.Message}");
        }
    }

    public async Task<List<ContactTypeDto>> GetContactTypesAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var contactTypes = await repo.ContactTypes.GetAllAsync().ConfigureAwait(false);
        return contactTypes.Select(ct => new ContactTypeDto(ct.Id, ct.Name)).ToList();
    }
}


