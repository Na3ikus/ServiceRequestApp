using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Auth;

/// <summary>
/// Login page component.
/// </summary>
public partial class Login : BaseComponent
{
    private readonly LoginModel loginModel = new LoginModel();

    [Inject]
    private Microsoft.AspNetCore.Hosting.IWebHostEnvironment Env { get; set; } = null!;

    [Inject]
    private Microsoft.Extensions.Configuration.IConfiguration Configuration { get; set; } = null!;

    private string? ErrorMessage { get; set; }

    private bool IsLoading { get; set; }

    private bool ShowPassword { get; set; }

    private bool IsCapsLockOn { get; set; }

    private bool ShouldShowDemoAccounts => this.Configuration.GetValue<bool?>("AuthSettings:ShowDemoAccounts") ?? this.Env.IsDevelopment();

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

    private void FillDemoAccount(string username, string password)
    {
        this.loginModel.Username = username;
        this.loginModel.Password = password;
        this.ErrorMessage = null;
        this.IsCapsLockOn = false;
    }

    private async Task HandleLoginAsync()
    {
        this.ErrorMessage = null;
        this.IsLoading = true;
        await Task.Yield(); // Force UI update to show the loading spinner immediately

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
            else if (error == "Database connection is unavailable.")
            {
                this.ErrorMessage = this.L.CurrentLanguage == "uk"
                    ? "Немає зв'язку до БД"
                    : "No connection to the database.";
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
