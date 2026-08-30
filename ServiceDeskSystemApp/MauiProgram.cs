using Microsoft.Extensions.Logging;
using ServiceDeskSystemApp.Views;
using ServiceDeskSystemApp.Services;
using ServiceDeskSystemApp.ViewModels;

namespace ServiceDeskSystemApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<HttpClient>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ITicketService, TicketService>();
            builder.Services.AddSingleton<IProfileService, ProfileService>();

            // ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<TicketsViewModel>();
            builder.Services.AddTransient<TicketDetailViewModel>();
            builder.Services.AddTransient<CreateTicketViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Pages
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<TicketsPage>();
            builder.Services.AddTransient<TicketDetailPage>();
            builder.Services.AddTransient<CreateTicketPage>();
            builder.Services.AddTransient<ProfilePage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
