using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var registerPage = Application.Current!
            .Handler!.MauiContext!.Services
            .GetRequiredService<RegisterPage>();
        await Navigation.PushAsync(registerPage);
    }
}
