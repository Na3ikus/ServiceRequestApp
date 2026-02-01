using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Localization;

namespace ServiceDeskSystem.Components.Pages;

/// <summary>
/// Registration page component.
/// </summary>
public partial class Register : IDisposable
{
    private readonly RegisterModel registerModel = new ();
    private bool disposed;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private ILocalizationService L { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private string? errorMessage { get; set; }

    private string? successMessage { get; set; }

    private bool isLoading { get; set; }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

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

    private async Task HandleRegisterAsync()
    {
        this.errorMessage = null;
        this.successMessage = null;
        this.isLoading = true;

        if (this.registerModel.Password != this.registerModel.ConfirmPassword)
        {
            this.errorMessage = this.L.CurrentLanguage == "uk"
                ? "Паролі не співпадають."
                : "Passwords do not match.";
            this.isLoading = false;
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
            this.successMessage = this.L.CurrentLanguage == "uk"
                ? "Реєстрація успішна! Перенаправляємо на сторінку входу..."
                : "Registration successful! Redirecting to login...";

            await Task.Delay(1500);
            this.Navigation.NavigateTo("/login");
        }
        else
        {
            this.errorMessage = error ?? "Registration failed. Please try again.";
        }

        this.isLoading = false;
    }

    private sealed class RegisterModel
    {
        [Required(ErrorMessage = "First name is required")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
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
