using System.Net.Mail;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

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
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IEmailSender EmailSender { get; set; } = null!;

    private string smtpTestRecipient { get; set; } = string.Empty;

    private string smtpTestSubject { get; set; } = "ServiceDesk SMTP test";

    private bool isSendingTestEmail { get; set; }

    private bool? smtpSendSuccess { get; set; }

    private string? smtpSendMessage { get; set; }

    private async Task SendSmtpTestEmailAsync()
    {
        if (this.isSendingTestEmail)
        {
            return;
        }

        var recipient = this.smtpTestRecipient.Trim();
        if (string.IsNullOrWhiteSpace(recipient) || !MailAddress.TryCreate(recipient, out _))
        {
            this.smtpSendSuccess = false;
            this.smtpSendMessage = "Enter a valid recipient email.";
            await this.ToastService.ShowToastAsync(this.smtpSendMessage, ToastType.Warning).ConfigureAwait(false);
            return;
        }

        this.isSendingTestEmail = true;
        this.smtpSendMessage = null;

        try
        {
            var subject = string.IsNullOrWhiteSpace(this.smtpTestSubject) ? "ServiceDesk SMTP test" : this.smtpTestSubject.Trim();
            var utcNow = DateTime.UtcNow;
            var textBody = $"SMTP test email from ServiceDeskSystem at {utcNow:O}.";
            var htmlBody = $"<p><strong>SMTP test email</strong> from ServiceDeskSystem.</p><p>UTC: {utcNow:O}</p>";

            await this.EmailSender.SendAsync(recipient, subject, htmlBody, textBody).ConfigureAwait(false);

            this.smtpSendSuccess = true;
            this.smtpSendMessage = "Test email sent successfully.";
            await this.ToastService.ShowToastAsync(this.smtpSendMessage, ToastType.Success).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.smtpSendSuccess = false;
            this.smtpSendMessage = ex.Message;
            await this.ToastService.ShowToastAsync($"Test email failed: {ex.Message}", ToastType.Error).ConfigureAwait(false);
        }
        finally
        {
            this.isSendingTestEmail = false;
        }
    }
}

