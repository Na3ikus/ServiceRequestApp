using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Tickets;
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

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    protected TicketCreateModel Model { get; set; } = new ();

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
            this.Navigation.NavigateTo($"/ticket/{created.Id}");
        }
        finally
        {
            this.IsSubmitting = false;
        }
    }

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

    protected RenderFragment? ErrorFor(string field) => this.Errors.TryGetValue(field, out var key)
        ? builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "role", "alert");
            builder.AddAttribute(2, "data-error", field);
            builder.AddAttribute(3, "class", "mt-1.5 text-13 text-danger");
            builder.AddContent(4, this.L.Translate(key));
            builder.CloseElement();
        }
        : null;

    private void ClearError(string field)
    {
        if (this.Errors.ContainsKey(field))
        {
            this.Errors = this.Errors.Where(e => e.Key != field).ToDictionary(e => e.Key, e => e.Value);
        }
    }

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
