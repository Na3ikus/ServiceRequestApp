using Microsoft.Extensions.DependencyInjection;
using ServiceDeskSystemApp.Views;
using ServiceDeskSystemApp.Services;

namespace ServiceDeskSystemApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();

            if (authService.IsAuthenticated)
            {
                return new Window(new AppShell());
            }
            else
            {
                var loginPage = _serviceProvider.GetRequiredService<LoginPage>();
                return new Window(new NavigationPage(loginPage));
            }
        }
    }
}