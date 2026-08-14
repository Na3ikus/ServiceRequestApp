using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Localization;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Profile;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Theme;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Toasts;

namespace ServiceDeskSystem.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddCoreServices();
        services.AddUIServices();

        return services;
    }

    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
        services.AddScoped<ServiceDeskSystem.Application.Common.IDomainEventDispatcher, ServiceDeskSystem.Application.Common.MediatRDomainEventDispatcher>();

        services.AddScoped<ITicketService, TicketService>();
        services.AddScoped<ITicketAssignmentService, TicketService>();
        services.AddScoped<ITicketStatisticsService, TicketService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ServiceDeskSystem.Application.Services.WorkLogs.IWorkLogService, ServiceDeskSystem.Application.Services.WorkLogs.WorkLogService>();
        services.AddScoped<ServiceDeskSystem.Application.Services.Attachments.IAttachmentService, ServiceDeskSystem.Application.Services.Attachments.AttachmentService>();
        services.TryAddSingleton<IRealtimeNotifier>(NoOpRealtimeNotifier.Instance);
        services.AddHostedService<SlaBackgroundService>();

        return services;
    }

    public static IServiceCollection AddUIServices(this IServiceCollection services)
    {
        services.AddScoped<ILocalizationService, LocalizationService>();
        services.AddScoped<IThemeService, ThemeService>();
        services.AddScoped<IToastService, ToastService>();

        return services;
    }
}

