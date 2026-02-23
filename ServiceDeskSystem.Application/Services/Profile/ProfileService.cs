using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Application.Services.Profile.Interfaces;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Infrastructure.Data;

namespace ServiceDeskSystem.Application.Services.Profile;

public sealed class ProfileService(BugTrackerDbContext context) : IProfileService
{
    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await context.Users
            .Include(u => u.Person)
            .ThenInclude(p => p.ContactInfos)
            .ThenInclude(ci => ci.ContactType)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

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
        var user = await context.Users
            .Include(u => u.Person)
            .ThenInclude(p => p.ContactInfos)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Person == null)
            return (false, "User not found.");

        user.Person.FirstName = request.FirstName;
        user.Person.LastName = request.LastName;
        user.Person.MiddleName = request.MiddleName;
        user.Person.Bio = request.Bio;

        // Update contacts
        var existingContacts = user.Person.ContactInfos.ToList();

        // Remove deleted contacts
        var requestContactIds = request.Contacts.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();
        var toRemove = existingContacts.Where(c => !requestContactIds.Contains(c.Id)).ToList();
        foreach (var r in toRemove)
        {
            context.ContactInfos.Remove(r);
        }

        // Add or update contacts
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
                context.ContactInfos.Add(newContact);
            }
        }

        try
        {
            await context.SaveChangesAsync();
            return (true, null);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to update profile: {ex.Message}");
        }
    }

    public async Task<List<ContactTypeDto>> GetContactTypesAsync()
    {
        return await context.ContactTypes
            .AsNoTracking()
            .Select(ct => new ContactTypeDto(ct.Id, ct.Name))
            .ToListAsync();
    }
}
