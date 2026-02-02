using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Services.Auth;

namespace ServiceDeskSystem.Components.Pages;

/// <summary>
/// Login page component.
/// </summary>
public partial class Login : BaseComponent
{
    private readonly LoginModel loginModel = new ();

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string? errorMessage { get; set; }

    private bool isLoading { get; set; }

    protected override void OnInitialized()
    {
        if (this.AuthService.IsAuthenticated)
        {
            this.Navigation.NavigateTo("/");
        }
    }

    private async Task HandleLoginAsync()
    {
        this.errorMessage = null;
        this.isLoading = true;

        this.loginModel.Username = this.loginModel.Username.Trim();
        this.loginModel.Password = this.loginModel.Password.Trim();

        var (success, error) = await this.AuthService.LoginAsync(this.loginModel.Username, this.loginModel.Password);

        if (success)
        {
            this.Navigation.NavigateTo("/");
        }
        else
        {
            this.errorMessage = error ?? "Login failed. Please try again.";
        }

        this.isLoading = false;
    }

    private sealed class LoginModel
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = string.Empty;
    }
}
