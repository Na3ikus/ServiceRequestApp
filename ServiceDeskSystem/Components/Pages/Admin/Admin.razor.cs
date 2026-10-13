#pragma warning disable CA1724
using System.Net.Mail;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Sla;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Components.Pages.Admin.Components;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Services.Presence;
using PathSegment = ServiceDeskSystem.Components.UI.Primitives.PathSegment;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Admin page: one route per section (products, tech stacks, users, who is online, tags, SLA, SMTP).
/// </summary>
public partial class Admin : BaseComponent
{
#pragma warning restore CA1724
    private const string DefaultSubject = "ServiceDesk SMTP test";

    private static readonly IReadOnlyList<PathSegment> DeniedSegments = [new ("admin")];

    private static readonly string[] Sections = ["products", "techstacks", "users", "online", "tags", "sla", "smtp"];

    private readonly List<SmtpLogEntry> smtpLog = [];

    /// <summary>
    /// Gets or sets the section from the URL (<c>/admin/{Section}</c>); unknown or missing shows products.
    /// </summary>
    [Parameter]
    public string? Section { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IEmailSender EmailSender { get; set; } = null!;

    [Inject]
    protected ITagService TagService { get; set; } = null!;

    [Inject]
    protected ISlaPolicyService SlaPolicyService { get; set; } = null!;

    [Inject]
    protected OnlineSessionTracker Presence { get; set; } = null!;

    [Inject]
    protected OnlineCircuitHandler Circuit { get; set; } = null!;

    protected IList<Product>? Products { get; set; }

    protected IList<TechStack>? TechStacks { get; set; }

    protected IList<User>? Users { get; set; }

    protected IList<Tag>? Tags { get; set; }

    protected IList<SlaPolicy>? SlaPolicies { get; set; }

    protected bool IsCheckingSmtp { get; set; }

    protected bool IsSendingTestEmail { get; set; }

    /// <summary>
    /// Gets the result of the last connection check; <c>null</c> before the first one finishes.
    /// </summary>
    protected bool? SmtpCheckSuccess { get; private set; }

    /// <summary>
    /// Gets the SMTP checks and test emails of this visit, oldest first.
    /// </summary>
    protected IReadOnlyList<SmtpLogEntry> SmtpLog => this.smtpLog;

    protected bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

    /// <summary>
    /// Gets the section shown: the URL's section when known, otherwise <c>products</c>.
    /// </summary>
    protected string ActiveSection
    {
        get
        {
            var match = Sections.FirstOrDefault(s => string.Equals(s, this.Section, StringComparison.OrdinalIgnoreCase));
            return match ?? Sections[0];
        }
    }

    protected IReadOnlyList<PathSegment> Segments => [new ("admin", "/admin"), new (this.ActiveSection)];

    protected override async Task OnInitializedAsync()
    {
        if (this.IsAdmin)
        {
            this.Presence.Changed += this.OnPresenceChanged;
            await this.LoadDataAsync();
            await this.CheckSmtpAsync();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.Presence.Changed -= this.OnPresenceChanged;
        }

        base.Dispose(disposing);
    }

    protected async Task LoadDataAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.TechStacks = await this.AdminService.GetAllTechStacksAsync();
        this.Products = await this.AdminService.GetAllProductsAsync();
        this.Users = await this.AdminService.GetAllUsersAsync();
        this.Tags = (await this.TagService.GetAllTagsAsync()).ToList();
        this.SlaPolicies = (await this.SlaPolicyService.GetAllPoliciesAsync()).ToList();
    }

    /// <summary>
    /// Runs <c>smtp check</c> and logs the result; the tab dot follows the last check.
    /// </summary>
    protected async Task CheckSmtpAsync()
    {
        if (this.IsCheckingSmtp)
        {
            return;
        }

        var entry = this.StartCommand("smtp check");
        this.IsCheckingSmtp = true;
        this.StateHasChanged();

        try
        {
            var (isSuccess, message) = await this.EmailSender.CheckConnectionAsync();
            this.SmtpCheckSuccess = isSuccess;
            entry.Complete(isSuccess, isSuccess ? this.Heading("admin.smtp.connected") : $"{this.Heading("admin.smtp.noConnection")} · {message}");
        }
        catch (Exception ex)
        {
            this.SmtpCheckSuccess = false;
            entry.Complete(false, $"{this.Heading("admin.smtp.noConnection")} · {ex.Message}");
        }
        finally
        {
            this.IsCheckingSmtp = false;
        }
    }

    /// <summary>
    /// Runs <c>smtp send</c> for a test email and logs the result.
    /// </summary>
    /// <param name="email">The recipient and subject from the form.</param>
    protected async Task SendTestEmailAsync(SmtpTestEmail email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (this.IsSendingTestEmail)
        {
            return;
        }

        var recipient = email.To.Trim();
        var subject = string.IsNullOrWhiteSpace(email.Subject) ? DefaultSubject : email.Subject.Trim();
        var entry = this.StartCommand($"smtp send {recipient} \"{subject}\"");

        if (string.IsNullOrWhiteSpace(recipient) || !MailAddress.TryCreate(recipient, out _))
        {
            entry.Complete(false, this.Heading("admin.smtp.invalidRecipient"));
            return;
        }

        this.IsSendingTestEmail = true;
        this.StateHasChanged();

        try
        {
            var utcNow = DateTime.UtcNow;
            var textBody = $"SMTP test email from ServiceDeskSystem at {utcNow:O}.";
            var htmlBody = $"<p><strong>SMTP test email</strong> from ServiceDeskSystem.</p><p>UTC: {utcNow:O}</p>";

            await this.EmailSender.SendAsync(recipient, subject, htmlBody, textBody);
            entry.Complete(true, this.Heading("admin.smtp.sent"));
        }
        catch (Exception ex)
        {
            entry.Complete(false, $"{this.Heading("admin.smtp.notSent")} · {ex.Message}");
        }
        finally
        {
            this.IsSendingTestEmail = false;
        }
    }

    private int? Count(string section) => section switch
    {
        "products" => this.Products?.Count,
        "techstacks" => this.TechStacks?.Count,
        "users" => this.Users?.Count,
        "tags" => this.Tags?.Count,
        "online" => this.Presence.Sessions.Count,
        "sla" => this.SlaPolicies?.Count,
        _ => null,
    };

    private (string State, string DotClass, string Text) SmtpState()
    {
        if (this.IsCheckingSmtp)
        {
            return ("checking", "bg-muted", this.L.Translate("admin.smtp.checking"));
        }

        return this.SmtpCheckSuccess switch
        {
            true => ("ok", "bg-accent ring-1 ring-accent-ink/40", this.L.Translate("admin.smtp.connected")),
            false => ("fail", "bg-danger", this.L.Translate("admin.smtp.noConnection")),
            _ => ("unknown", "bg-muted", this.L.Translate("admin.smtp.unknown")),
        };
    }

    // Raised on the thread of the circuit that connected, signed in or left.
    private void OnPresenceChanged(object? sender, EventArgs e) => _ = this.InvokeAsync(this.StateHasChanged);

    private SmtpLogEntry StartCommand(string command)
    {
        var entry = new SmtpLogEntry(command);
        this.smtpLog.Add(entry);
        return entry;
    }
}
