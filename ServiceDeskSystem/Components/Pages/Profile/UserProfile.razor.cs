using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Profile.Interfaces;
using ServiceDeskSystem.Application.Services.Profile.Models;
using ServiceDeskSystem.Components.Common.Base;

namespace ServiceDeskSystem.Components.Pages.Profile;

public partial class UserProfile : BaseComponent
{
    private bool isLoading = true;
    private bool isSaving;
    private bool isSaved;
    private string? errorMessage;
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
        if (this.model == null)
        {
            return;
        }

        this.isSaving = true;
        this.isSaved = false;
        this.errorMessage = null;

        try
        {
            var user = this.AuthService.CurrentUser;
            if (user != null)
            {
                var (success, error) = await this.ProfileService.UpdateProfileAsync(user.Id, this.model);
                if (success)
                {
                    this.isSaved = true;

                    // Reset saved message after 3 seconds
                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(3000);
                        this.isSaved = false;
                        await this.InvokeAsync(this.StateHasChanged);
                    });
                }
                else
                {
                    this.errorMessage = error;
                }
            }
        }
        catch (Exception ex)
        {
            this.errorMessage = $"Помилка збереження: {ex.Message}";
        }
        finally
        {
            this.isSaving = false;
        }
    }
}
