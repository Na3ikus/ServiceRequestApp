using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ServiceDeskSystemApp.Models.Auth;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IProfileService _profileService;
    private readonly IAuthService _authService;

    public ProfileViewModel(IProfileService profileService, IAuthService authService)
    {
        _profileService = profileService;
        _authService = authService;
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
        // Go back to Login
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
