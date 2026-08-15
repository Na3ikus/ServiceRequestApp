using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Auth;

/// <summary>
/// Registration page component.
/// </summary>
public partial class Register : BaseComponent
{
    private readonly RegisterModel registerModel = new RegisterModel();

    private string? ErrorMessage { get; set; }

    private string? SuccessMessage { get; set; }

    private bool IsLoading { get; set; }

    private bool ShowPassword { get; set; }

    private bool ShowConfirmPassword { get; set; }

    private bool IsCapsLockOn { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (this.AuthService.IsAuthenticated)
        {
            this.Navigation.NavigateTo("/");
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await this.Theme.InitializeAsync();
            this.StateHasChanged();
        }
    }

    private void TogglePasswordVisibility()
    {
        this.ShowPassword = !this.ShowPassword;
    }

    private void ToggleConfirmPasswordVisibility()
    {
        this.ShowConfirmPassword = !this.ShowConfirmPassword;
    }

    private void HandlePasswordKeyDown(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e) => this.CheckCapsLock(e);

    private void HandlePasswordKeyUp(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e) => this.CheckCapsLock(e);

    private void CheckCapsLock(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Key) && e.Key.Length == 1 && char.IsLetter(e.Key[0]))
        {
            var isUpper = char.IsUpper(e.Key[0]);
            this.IsCapsLockOn = (isUpper && !e.ShiftKey) || (!isUpper && e.ShiftKey);
        }
        else if (e.Key == "CapsLock")
        {
            this.IsCapsLockOn = !this.IsCapsLockOn;
        }
    }

    private async Task HandleRegisterAsync()
    {
        this.ErrorMessage = null;
        this.SuccessMessage = null;
        this.IsLoading = true;

        if (this.registerModel.Password != this.registerModel.ConfirmPassword)
        {
            this.ErrorMessage = this.L.Translate("register.passwordsMismatch");
            this.IsLoading = false;
            return;
        }

        var (success, error) = await this.AuthService.RegisterClientAsync(
            this.registerModel.Username.Trim(),
            this.registerModel.Password,
            this.registerModel.FirstName.Trim(),
            this.registerModel.LastName.Trim(),
            this.registerModel.Email?.Trim());

        if (success)
        {
            this.SuccessMessage = this.L.Translate("register.successRedirect");

            await Task.Delay(1500);
            this.Navigation.NavigateTo("/login");
        }
        else
        {
            if (error == "Username already exists.")
            {
                this.ErrorMessage = this.L.Translate("register.usernameExists");
            }
            else if (error == "Email address is already registered.")
            {
                this.ErrorMessage = this.L.Translate("register.emailExists");
            }
            else if (error == "Database connection is unavailable.")
            {
                this.ErrorMessage = this.L.Translate("login.dbUnavailable");
            }
            else
            {
                this.ErrorMessage = error ?? "Registration failed. Please try again.";
            }
        }

        this.IsLoading = false;
    }

    private sealed class RegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
        [RegularExpression(@"^\S+$", ErrorMessage = "Username cannot contain spaces")]
        public string Username { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$",
            ErrorMessage = "Password must contain uppercase, lowercase, digit and special character")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
