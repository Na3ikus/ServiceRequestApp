using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Components.Features;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Auth;

/// <summary>
/// Registration page component.
/// </summary>
public partial class Register : BaseComponent
{
    private readonly RegisterModel registerModel = new RegisterModel();

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string? ErrorMessage { get; set; }

    private string? SuccessMessage { get; set; }

    private bool IsLoading { get; set; }

    private bool ShowPassword { get; set; }

    private bool ShowConfirmPassword { get; set; }

    protected override void OnInitialized()
    {
        if (this.AuthService.IsAuthenticated)
        {
            this.Navigation.NavigateTo("/");
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

    private async Task HandleRegisterAsync()
    {
        this.ErrorMessage = null;
        this.SuccessMessage = null;
        this.IsLoading = true;

        if (this.registerModel.Password != this.registerModel.ConfirmPassword)
        {
            this.ErrorMessage = this.L.CurrentLanguage == "uk"
                ? "Паролі не співпадають."
                : "Passwords do not match.";
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
            this.SuccessMessage = this.L.CurrentLanguage == "uk"
                ? "Реєстрація успішна! Перенаправляємо на сторінку входу..."
                : "Registration successful! Redirecting to login...";

            await Task.Delay(1500);
            this.Navigation.NavigateTo("/login");
        }
        else
        {
            if (error == "Username already exists.")
            {
                this.ErrorMessage = this.L.CurrentLanguage == "uk"
                    ? "Користувач з таким логіном вже існує."
                    : error;
            }
            else if (error == "Email address is already registered.")
            {
                this.ErrorMessage = this.L.CurrentLanguage == "uk"
                    ? "Ця електронна адреса вже зареєстрована."
                    : error;
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
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm your password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
