using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;

namespace ServiceDeskSystem.Application.Services.Tickets;

/// <summary>
/// Background service that periodically checks for SLA warnings and breaches on active tickets.
/// </summary>
public sealed class SlaBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SlaBackgroundService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

    public SlaBackgroundService(IServiceProvider serviceProvider, ILogger<SlaBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SLA Background Service is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckSlaAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking SLA.");
            }

            try
            {
                await Task.Delay(_checkInterval, stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("SLA Background Service is stopping.");
    }

    public async Task CheckSlaAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var repositoryFactory = scope.ServiceProvider.GetRequiredService<IRepositoryFacadeFactory>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var realtimeNotifier = scope.ServiceProvider.GetRequiredService<IRealtimeNotifier>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        await using var repo = repositoryFactory.Create();
        var tickets = await repo.Tickets.GetActiveTicketsForSlaAsync().ConfigureAwait(false);
        bool changesMade = false;

        foreach (var ticket in tickets)
        {
            if (!ticket.DueDate.HasValue) continue;

            var now = DateTime.UtcNow;

            // 1. SLA Breach Check
            if (now > ticket.DueDate.Value && !ticket.IsSlaBreached)
            {
                _logger.LogWarning("Ticket #{Id} has breached SLA. Due Date was {DueDate}.", ticket.Id, ticket.DueDate.Value);
                ticket.IsSlaBreached = true;
                changesMade = true;

                // Send in-app notification to assignee and author
                var message = $"SLA BREACHED: Ticket #{ticket.Id} ('{ticket.Title}') is overdue!";
                await SendSlaNotificationsAsync(ticket, "SlaBreached", message, notificationService, emailSender, cancellationToken).ConfigureAwait(false);
            }
            // 2. SLA Warning Check (2 hours for Critical, 24 hours for others)
            else if (!ticket.IsSlaBreached && !ticket.SlaWarningSent)
            {
                var warningThreshold = ticket.Priority == TicketPriority.Critical
                    ? ticket.DueDate.Value.AddHours(-2)
                    : ticket.DueDate.Value.AddDays(-1);

                if (now > warningThreshold)
                {
                    _logger.LogInformation("Ticket #{Id} is approaching SLA breach. Due Date is {DueDate}.", ticket.Id, ticket.DueDate.Value);
                    ticket.SlaWarningSent = true;
                    changesMade = true;

                    // Send in-app notification to assignee and author
                    var message = $"SLA WARNING: Ticket #{ticket.Id} ('{ticket.Title}') is approaching its due date!";
                    await SendSlaNotificationsAsync(ticket, "SlaWarning", message, notificationService, emailSender, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        if (changesMade)
        {
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
            await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        }
    }

    private async Task SendSlaNotificationsAsync(
        Ticket ticket,
        string type,
        string message,
        INotificationService notificationService,
        IEmailSender emailSender,
        CancellationToken cancellationToken)
    {
        // 1. In-app notifications
        if (ticket.DeveloperId.HasValue)
        {
            await notificationService.CreateSlaNotificationAsync(ticket.Id, type, message, ticket.DeveloperId.Value).ConfigureAwait(false);
        }
        await notificationService.CreateSlaNotificationAsync(ticket.Id, type, message, ticket.AuthorId).ConfigureAwait(false);

        // 2. Email notifications
        var subject = type == "SlaBreached" ? "SLA Breached Alert" : "SLA Warning Alert";
        var emailBody = $"<h3>{subject}</h3><p>{message}</p><p>Priority: {ticket.Priority}<br>Due Date: {ticket.DueDate!.Value:yyyy-MM-dd HH:mm:ss} UTC</p>";

        // Send to assigned developer
        if (ticket.DeveloperId.HasValue && ticket.Developer != null)
        {
            var devEmail = GetUserEmail(ticket.Developer);
            if (!string.IsNullOrWhiteSpace(devEmail))
            {
                await SendEmailSafeAsync(emailSender, devEmail, subject, emailBody, cancellationToken).ConfigureAwait(false);
            }
        }

        // Send to author
        if (ticket.Author != null)
        {
            var authorEmail = GetUserEmail(ticket.Author);
            if (!string.IsNullOrWhiteSpace(authorEmail))
            {
                await SendEmailSafeAsync(emailSender, authorEmail, subject, emailBody, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private async Task SendEmailSafeAsync(IEmailSender emailSender, string email, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        try
        {
            await emailSender.SendAsync(email, subject, htmlBody, htmlBody, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send SLA email notification to {Email}.", email);
        }
    }

    private string? GetUserEmail(User? user)
    {
        if (user?.Person?.ContactInfos == null) return null;

        var primaryEmail = user.Person.ContactInfos
            .FirstOrDefault(ci => ci.ContactType?.Name == "Email" && ci.IsPrimary);

        if (primaryEmail != null) return primaryEmail.Value;

        var anyEmail = user.Person.ContactInfos
            .FirstOrDefault(ci => ci.ContactType?.Name == "Email");

        return anyEmail?.Value;
    }
}
