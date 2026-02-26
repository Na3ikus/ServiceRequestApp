using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Admin.Interfaces;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Comments.Interfaces;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Localization.Interfaces;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Profile.Interfaces;
using ServiceDeskSystem.Application.Services.Theme;
using ServiceDeskSystem.Application.Services.Theme.Interfaces;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Tickets.Interfaces;

namespace ServiceDeskSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketAssignmentService, TicketService>();
        services.AddScoped<ITicketStatisticsService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAuthService, SimpleAuthService>();
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IProfileService, ProfileService>();

        return services;
    }
}
