using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using ServiceDeskSystemApp.Models.Auth;
using ServiceDeskSystemApp.Services;
using ServiceDeskSystemApp.Views;

namespace ServiceDeskSystemApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public ProfileViewModel(IProfileService profileService, IAuthService authService, IServiceProvider serviceProvider)
    {
        _profileService = profileService;
        _authService = authService;
        _serviceProvider = serviceProvider;
    }

    [ObservableProperty]
    private UserDto? _userProfile;

    [ObservableProperty]
    private bool _isLoading;

    [RelayCommand]
    public async Task LoadProfileAsync()
    {
        IsLoading = true;
        UserProfile = await _profileService.GetProfileAsync();
        IsLoading = false;
    }

    [RelayCommand]
    public async Task LogoutAsync()
    {
        await _authService.LogoutAsync();
        if (Application.Current != null && Application.Current.Windows.Count > 0)
        {
            var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
            Application.Current.Windows[0].Page = new NavigationPage(loginPage);
        }
    }
}
