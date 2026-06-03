using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Profile;

/// <summary>
/// User profile page component.
/// </summary>
public partial class UserProfile : BaseComponent
{
    private bool isLoading = true;
    private bool isSaving;
    private bool isSaved;
    private string? errorMessage;
    private string? warningMessage;
    private UpdateProfileRequest? model;
    private List<ContactTypeDto> contactTypes = new List<ContactTypeDto>();

    [Inject]
    public IAuthService AuthService { get; set; } = null!;

    [Inject]
    public IProfileService ProfileService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var user = this.AuthService.CurrentUser;
            if (user == null)
            {
                this.errorMessage = "Не авторизовано";
                this.isLoading = false;
                return;
            }

            this.contactTypes = await this.ProfileService.GetContactTypesAsync();
            var profileDto = await this.ProfileService.GetProfileAsync(user.Id);

            if (profileDto != null)
            {
                this.model = new UpdateProfileRequest
                {
                    FirstName = profileDto.FirstName,
                    LastName = profileDto.LastName,
                    MiddleName = profileDto.MiddleName,
                    Bio = profileDto.Bio,
                    Contacts = profileDto.Contacts.Select(c => new UpdateContactRequest
                    {
                        Id = c.Id,
                        Value = c.Value,
                        ContactTypeId = c.ContactTypeId,
                        IsPrimary = c.IsPrimary,
                    }).ToList(),
                };
            }
            else
            {
                this.errorMessage = "Профіль не знайдено";
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Помилка завантаження: {ex.Message}";
        }
        finally
        {
            this.isLoading = false;
        }
    }

    private void AddContact()
    {
        this.model?.Contacts.Add(new UpdateContactRequest
        {
            ContactTypeId = this.contactTypes.FirstOrDefault()?.Id ?? 0,
        });
    }

    private void RemoveContact(UpdateContactRequest contact)
    {
        this.model?.Contacts.Remove(contact);
    }

    private async Task HandleValidSubmit()
    {
        this.warningMessage = null;
        this.errorMessage = null;
        this.isSaved = false;

        if (this.model is null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(this.model.FirstName))
        {
            this.warningMessage = "profile.validation.firstNameRequired";
            return;
        }

        if (this.model.Contacts.Any(c => string.IsNullOrWhiteSpace(c.Value)))
        {
            this.warningMessage = "profile.validation.contactValueRequired";
            return;
        }

        var user = this.AuthService.CurrentUser;
        if (user is null)
        {
            this.errorMessage = "Не авторизовано";
            return;
        }

        this.isSaving = true;
        try
        {
            var result = await this.ProfileService.UpdateProfileAsync(user.Id, this.model);
            if (result.Success)
            {
                this.isSaved = true;
            }
            else
            {
                this.errorMessage = result.ErrorMessage ?? "Не вдалося зберегти профіль.";
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = ex.Message;
        }
        finally
        {
            this.isSaving = false;
        }
    }
}

