using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using ServiceDeskSystem.Application.Services.Attachments;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using PathSegment = ServiceDeskSystem.Components.UI.Primitives.PathSegment;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Create ticket page component.
/// </summary>
public partial class CreateTicket
{
    private static readonly IReadOnlyList<PathSegment> Segments = [new ("tickets"), new ("new")];

    private static readonly TicketType[] Types = [TicketType.Bug, TicketType.Support, TicketType.Consultation, TicketType.Project];

    private readonly List<PendingFile> files = [];

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    [Inject]
    protected IAttachmentService AttachmentService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected TicketCreateModel Model { get; set; } = new ();

    /// <summary>
    /// Gets the files picked so far; they are uploaded once the ticket exists.
    /// </summary>
    protected IReadOnlyList<PendingFile> Files => this.files;

    /// <summary>
    /// Gets the files refused by the last pick, as "name: reason" lines.
    /// </summary>
    protected IReadOnlyList<string> FileErrors { get; private set; } = [];

    protected IList<Product> Products { get; set; } = [];

    protected bool IsSubmitting { get; set; }

    protected bool IsCriticalConfirmed { get; set; }

    protected string? CriticalConfirmationError { get; set; }

    /// <summary>
    /// Gets the validation errors of the last submit: field → localization key.
    /// </summary>
    protected IReadOnlyDictionary<string, string> Errors { get; private set; } = new Dictionary<string, string>();

    protected int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected Product? SelectedProduct => this.Products.FirstOrDefault(p => p.Id == this.Model.ProductId);

    protected TicketPriority PreviewPriority => this.IsAdminOrDeveloper ? this.Model.Priority : TicketPriority.Medium;

    protected IReadOnlyList<(string Value, string Text)> TypeOptions =>
        Types.Select(t => (t.ToString(), this.Heading(TypeKey(t)))).ToList();

    protected static string TypeKey(TicketType type) => type switch
    {
        TicketType.Bug => "ticketType.bug",
        TicketType.Support => "ticketType.support",
        TicketType.Consultation => "ticketType.consultation",
        TicketType.Project => "ticketType.project",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };

    protected override async Task OnInitializedAsync()
    {
        this.Products = await this.TicketService.GetProductsAsync();
        this.Model.Priority = TicketPriority.Medium;
    }

    protected async Task HandleSubmitAsync()
    {
        if (this.IsSubmitting)
        {
            return;
        }

        this.CriticalConfirmationError = null;
        this.Errors = this.Model.Validate();
        if (this.Errors.Count > 0 || this.Model.TicketType is not { } type)
        {
            return;
        }

        bool isDevOrAdmin = this.IsAdminOrDeveloper;
        var priority = isDevOrAdmin ? this.Model.Priority : TicketPriority.Medium;

        if (isDevOrAdmin && priority == TicketPriority.Critical && !this.IsCriticalConfirmed)
        {
            this.CriticalConfirmationError = this.L.Translate("create.criticalRequired");
            return;
        }

        this.IsSubmitting = true;

        var ticket = Ticket.Create(
            this.Model.Title.Trim(),
            this.Model.Description,
            type,
            priority,
            this.CurrentUserId,
            type == TicketType.Project ? null : this.Model.ProductId,
            isPriorityAssessed: isDevOrAdmin);

        if (type == TicketType.Bug)
        {
            ticket.StepsToReproduce = this.Model.StepsToReproduce ?? string.Empty;
            ticket.Environment = this.Model.Environment ?? string.Empty;
            ticket.AffectedVersion = this.Model.AffectedVersion ?? string.Empty;
        }

        try
        {
            var created = await this.TicketService.CreateTicketAsync(ticket);

            var failed = await this.UploadFilesAsync(created.Id);
            if (failed > 0)
            {
                // The ticket exists; the files can be added again on its page.
                await this.ToastService.ShowToastAsync(
                    string.Format(this.UiCulture, this.L.Translate("create.filesNotAttached"), failed),
                    ToastType.Warning);
            }

            this.Navigation.NavigateTo($"/ticket/{created.Id}");
        }
        finally
        {
            this.IsSubmitting = false;
        }
    }

    /// <summary>
    /// Keeps the picked files that pass <see cref="AttachmentRules"/>, read into memory right away:
    /// a later pick replaces the browser's file list, so the earlier files could not be read on submit.
    /// </summary>
    protected async Task OnFilesPickedAsync(InputFileChangeEventArgs e)
    {
        var errors = new List<string>();

        // Allow a large pick here so the extra files get a message instead of an exception.
        foreach (var file in e.GetMultipleFiles(maximumFileCount: 100))
        {
            if (this.files.Count >= AttachmentRules.MaxFilesOnCreate)
            {
                errors.Add($"{file.Name}: {this.L.Translate("create.tooManyFiles")}");
            }
            else if (!AttachmentRules.IsAllowedType(file.Name))
            {
                errors.Add($"{file.Name}: {this.L.Translate("create.fileTypeNotAllowed")}");
            }
            else if (file.Size > AttachmentRules.MaxFileSize)
            {
                errors.Add($"{file.Name}: {this.L.Translate("details.fileTooLarge")}");
            }
            else
            {
                using var buffer = new MemoryStream();
                await using (var stream = file.OpenReadStream(AttachmentRules.MaxFileSize))
                {
                    await stream.CopyToAsync(buffer);
                }

                this.files.Add(new PendingFile(file.Name, file.ContentType, buffer.ToArray()));
            }
        }

        this.FileErrors = errors;
    }

    protected void RemoveFile(PendingFile file)
    {
        this.files.Remove(file);
        this.FileErrors = [];
    }

    protected static string FormatSize(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{Math.Round(bytes / 1024d, 1).ToString(System.Globalization.CultureInfo.InvariantCulture)} KB",
        _ => $"{Math.Round(bytes / 1024d / 1024d, 1).ToString(System.Globalization.CultureInfo.InvariantCulture)} MB",
    };

    protected void Cancel()
    {
        this.Navigation.NavigateTo("/");
    }

    protected void SelectType(string value)
    {
        if (!Enum.TryParse<TicketType>(value, out var type))
        {
            return;
        }

        this.Model.TicketType = type;
        if (type == TicketType.Project)
        {
            this.Model.ProductId = null;
        }

        this.ClearError(TicketCreateModel.TypeField);
        this.ClearError(TicketCreateModel.ProductField);
    }

    protected void OnPriorityChanged()
    {
        this.IsCriticalConfirmed = false;
        this.CriticalConfirmationError = null;
    }

    protected string? Invalid(string field) => this.Errors.ContainsKey(field) ? "true" : null;

    /// <summary>
    /// Id of the field's validation message while it is shown (for <c>aria-describedby</c>).
    /// </summary>
    protected string? DescribedBy(string field) => this.Errors.ContainsKey(field) ? ErrorId(field) : null;

    protected RenderFragment? ErrorFor(string field) => this.Errors.TryGetValue(field, out var key)
        ? builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "id", ErrorId(field));
            builder.AddAttribute(2, "role", "alert");
            builder.AddAttribute(3, "data-error", field);
            builder.AddAttribute(4, "class", "mt-1.5 text-13 text-danger");
            builder.AddContent(5, this.L.Translate(key));
            builder.CloseElement();
        }
        : null;

    private static string ErrorId(string field) => $"create-error-{field}";

    /// <summary>
    /// Uploads the picked files to the new ticket; returns how many failed.
    /// </summary>
    private async Task<int> UploadFilesAsync(int ticketId)
    {
        var failed = 0;
        foreach (var file in this.files)
        {
            try
            {
                using var stream = new MemoryStream(file.Content, writable: false);
                var result = await this.AttachmentService.UploadAttachmentAsync(ticketId, file.Name, file.ContentType, stream, this.CurrentUserId);
                if (!result.Success)
                {
                    failed++;
                }
            }
            catch (IOException)
            {
                failed++;
            }
            catch (UnauthorizedAccessException)
            {
                failed++;
            }
        }

        return failed;
    }

    private void ClearError(string field)
    {
        if (this.Errors.ContainsKey(field))
        {
            this.Errors = this.Errors.Where(e => e.Key != field).ToDictionary(e => e.Key, e => e.Value);
        }
    }

    /// <summary>
    /// A file picked on the form, held in memory until the ticket is created.
    /// </summary>
    protected sealed record PendingFile(string Name, string ContentType, byte[] Content);

    /// <summary>
    /// What the create form collects. No type is chosen until the user picks one.
    /// </summary>
    protected sealed class TicketCreateModel
    {
        public const string TypeField = "type";
        public const string TitleField = "title";
        public const string DescriptionField = "description";
        public const string ProductField = "product";

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; }

        public TicketType? TicketType { get; set; }

        public int? ProductId { get; set; }

        public string? StepsToReproduce { get; set; }

        public string? Environment { get; set; }

        public string? AffectedVersion { get; set; }

        /// <summary>
        /// Checks the form; returns field → localization key of the message (empty when valid).
        /// </summary>
        public IReadOnlyDictionary<string, string> Validate()
        {
            var errors = new Dictionary<string, string>();

            if (this.TicketType is null)
            {
                errors[TypeField] = "create.typeRequired";
            }

            var title = this.Title.Trim();
            if (title.Length == 0)
            {
                errors[TitleField] = "create.titleRequired";
            }
            else if (title.Length is < 5 or > 200)
            {
                errors[TitleField] = "create.titleLength";
            }

            if (string.IsNullOrWhiteSpace(this.Description))
            {
                errors[DescriptionField] = "create.descriptionRequired";
            }
            else if (this.Description.Length is < 10 or > 5000)
            {
                errors[DescriptionField] = "create.descriptionLength";
            }

            // Product is required for every type except project (also before a type is picked).
            if (this.TicketType != ServiceDeskSystem.Domain.Enums.TicketType.Project && !this.ProductId.HasValue)
            {
                errors[ProductField] = "create.productRequired";
            }

            return errors;
        }
    }
}
