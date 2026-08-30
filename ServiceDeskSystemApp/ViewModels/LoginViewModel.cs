using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [RelayCommand]
    public async Task LoginAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _authService.LoginAsync(Username, Password);
            if (response == null)
            {
                ErrorMessage = "Login failed. Please check your credentials.";
            }
            else
            {
                // Switch the main page to AppShell after successful login
                if (Application.Current != null)
                {
                    Application.Current.Windows[0].Page = new AppShell();
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        
        IsLoading = false;
    }
}
