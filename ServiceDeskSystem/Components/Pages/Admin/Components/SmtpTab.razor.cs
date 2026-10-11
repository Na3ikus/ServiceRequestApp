using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.UI.Base;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// SMTP section: a terminal log of checks and test emails, and the test email form.
/// The log and the commands live in <see cref="Admin"/> for the time the page is open.
/// </summary>
public partial class SmtpTab : BaseComponent
{
    private string recipient = string.Empty;
    private string subject = "ServiceDesk SMTP test";

    /// <summary>
    /// Gets or sets the log entries, oldest first.
    /// </summary>
    [Parameter]
    public IReadOnlyList<SmtpLogEntry> Log { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether a connection check is running.
    /// </summary>
    [Parameter]
    public bool IsChecking { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a test email is being sent.
    /// </summary>
    [Parameter]
    public bool IsSending { get; set; }

    /// <summary>
    /// Gets or sets the callback that runs a connection check.
    /// </summary>
    [Parameter]
    public EventCallback OnCheck { get; set; }

    /// <summary>
    /// Gets or sets the callback that sends a test email.
    /// </summary>
    [Parameter]
    public EventCallback<SmtpTestEmail> OnSend { get; set; }

    private Task SendAsync() => this.IsSending ? Task.CompletedTask : this.OnSend.InvokeAsync(new SmtpTestEmail(this.recipient, this.subject));
}
