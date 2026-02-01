using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Localization;

namespace ServiceDeskSystem.Components.Pages;

/// <summary>
/// Login page component.
/// </summary>
public partial class Login : IDisposable
{
    private readonly LoginModel loginModel = new ();
    private bool disposed;

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private ILocalizationService L { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string? errorMessage { get; set; }

    private bool isLoading { get; set; }

    protected override void OnInitialized()
    {
        this.L.LanguageChanged += this.OnStateChanged;

        if (this.AuthService.IsAuthenticated)
        {
            this.Navigation.NavigateTo("/");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.L.LanguageChanged -= this.OnStateChanged;
        }

        this.disposed = true;
    }

    private void OnStateChanged(object? sender, EventArgs e) => this.InvokeAsync(this.StateHasChanged);

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
