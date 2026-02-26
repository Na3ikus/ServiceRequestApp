using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Components.Common.Base;

namespace ServiceDeskSystem.Components.Pages.Auth;

/// <summary>
/// Login page component.
/// </summary>
public partial class Login : BaseComponent
{
    private readonly LoginModel loginModel = new LoginModel();

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string? ErrorMessage { get; set; }

    private bool IsLoading { get; set; }

    private bool ShowPassword { get; set; }

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

    private async Task HandleLoginAsync()
    {
        this.ErrorMessage = null;
        this.IsLoading = true;

        this.loginModel.Username = this.loginModel.Username.Trim();
        this.loginModel.Password = this.loginModel.Password.Trim();

        var (success, error) = await this.AuthService.LoginAsync(this.loginModel.Username, this.loginModel.Password);

        if (success)
        {
            this.Navigation.NavigateTo("/");
        }
        else
        {
            if (error == "Invalid username or password.")
            {
                this.ErrorMessage = this.L.CurrentLanguage == "uk"
                    ? "Невірний логін або пароль."
                    : error;
            }
            else if (error == "Account is deactivated. Please contact administrator.")
            {
                this.ErrorMessage = this.L.CurrentLanguage == "uk"
                    ? "Акаунт деактивовано. Будь ласка, зверніться до адміністратора."
                    : error;
            }
            else
            {
                this.ErrorMessage = error ?? "Login failed. Please try again.";
            }
        }

        this.IsLoading = false;
    }

    private sealed class LoginModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
