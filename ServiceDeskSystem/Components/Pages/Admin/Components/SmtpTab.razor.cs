using System.Net.Mail;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// SMTP configuration and diagnostics tab component.
/// </summary>
public partial class SmtpTab : BaseComponent
{
    [Parameter]
    public bool? SmtpCheckSuccess { get; set; }

    [Parameter]
    public string? SmtpCheckMessage { get; set; }

    [Parameter]
    public bool IsCheckingSmtp { get; set; }

    [Parameter]
    public EventCallback OnCheckSmtp { get; set; }

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    [Inject]
    protected IEmailSender EmailSender { get; set; } = null!;

    protected string SmtpTestRecipient { get; set; } = string.Empty;

    protected string SmtpTestSubject { get; set; } = "ServiceDesk SMTP test";

    protected bool IsSendingTestEmail { get; set; }

    protected bool? SmtpSendSuccess { get; set; }

    protected string? SmtpSendMessage { get; set; }

    protected async Task SendSmtpTestEmailAsync()
    {
        if (this.IsSendingTestEmail)
        {
            return;
        }

        var recipient = this.SmtpTestRecipient.Trim();
        if (string.IsNullOrWhiteSpace(recipient) || !MailAddress.TryCreate(recipient, out _))
        {
            this.SmtpSendSuccess = false;
            this.SmtpSendMessage = "Enter a valid recipient email.";
            await this.ToastService.ShowToastAsync(this.SmtpSendMessage, ToastType.Warning).ConfigureAwait(false);
            return;
        }

        this.IsSendingTestEmail = true;
        this.SmtpSendMessage = null;

        try
        {
            var subject = string.IsNullOrWhiteSpace(this.SmtpTestSubject) ? "ServiceDesk SMTP test" : this.SmtpTestSubject.Trim();
            var utcNow = DateTime.UtcNow;
            var textBody = $"SMTP test email from ServiceDeskSystem at {utcNow:O}.";
            var htmlBody = $"<p><strong>SMTP test email</strong> from ServiceDeskSystem.</p><p>UTC: {utcNow:O}</p>";

            await this.EmailSender.SendAsync(recipient, subject, htmlBody, textBody).ConfigureAwait(false);

            this.SmtpSendSuccess = true;
            this.SmtpSendMessage = "Test email sent successfully.";
            await this.ToastService.ShowToastAsync(this.SmtpSendMessage, ToastType.Success).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.SmtpSendSuccess = false;
            this.SmtpSendMessage = ex.Message;
            await this.ToastService.ShowToastAsync($"Test email failed: {ex.Message}", ToastType.Error).ConfigureAwait(false);
        }
        finally
        {
            this.IsSendingTestEmail = false;
        }
    }
}
