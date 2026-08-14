using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Create ticket page component.
/// </summary>
public partial class CreateTicket
{
    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    protected TicketCreateModel Model { get; set; } = new ();

    protected IList<Product> Products { get; set; } = [];

    protected bool IsSubmitting { get; set; }

    protected bool IsTypeSelected { get; set; }

    protected string? ProductValidationError { get; set; }

    protected bool IsCriticalConfirmed { get; set; }

    protected string? CriticalConfirmationError { get; set; }

    protected bool IsProductRequired => this.Model.TicketType != TicketType.Project;

    protected int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected override async Task OnInitializedAsync()
    {
        this.Products = await this.TicketService.GetProductsAsync();
        this.Model.Priority = TicketPriority.Medium;
    }

    protected async Task HandleSubmitAsync()
    {
        this.ProductValidationError = null;
        this.CriticalConfirmationError = null;

        if (this.IsProductRequired && !this.Model.ProductId.HasValue)
        {
            this.ProductValidationError = "Please select a product";
            return;
        }

        var role = this.AuthService.CurrentUser?.Role;
        bool isDevOrAdmin = role is UserRole.Admin or UserRole.Developer;
        var priority = isDevOrAdmin ? this.Model.Priority : TicketPriority.Medium;

        if (isDevOrAdmin && priority == TicketPriority.Critical && !this.IsCriticalConfirmed)
        {
            this.CriticalConfirmationError = this.L.Translate("create.criticalRequired");
            return;
        }

        this.IsSubmitting = true;
        bool isPriorityAssessed = isDevOrAdmin;

        var ticket = Ticket.Create(
            this.Model.Title,
            this.Model.Description,
            this.Model.TicketType,
            priority,
            this.CurrentUserId,
            this.Model.ProductId,
            isPriorityAssessed);

        ticket.StepsToReproduce = this.Model.StepsToReproduce ?? string.Empty;
        ticket.Environment = this.Model.Environment ?? string.Empty;
        ticket.AffectedVersion = this.Model.AffectedVersion ?? string.Empty;

        await this.TicketService.CreateTicketAsync(ticket);

        this.Navigation.NavigateTo("/");
    }

    protected void Cancel()
    {
        this.Navigation.NavigateTo("/");
    }

    protected void SelectType(TicketType type)
    {
        this.Model.TicketType = type;
        this.IsTypeSelected = true;
        this.OnTicketTypeChanged();
    }

    protected void BackToTypeSelection()
    {
        this.IsTypeSelected = false;
    }

    protected void OnTicketTypeChanged()
    {
        this.ProductValidationError = null;

        if (!this.IsProductRequired)
        {
            this.Model.ProductId = null;
        }
    }

    protected sealed class TicketCreateModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 5000 characters")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Priority is required")]
        public TicketPriority Priority { get; set; }

        [Required(ErrorMessage = "Ticket type is required")]
        public TicketType TicketType { get; set; }

        public int? ProductId { get; set; }

        public string? StepsToReproduce { get; set; }

        public string? Environment { get; set; }

        public string? AffectedVersion { get; set; }
    }
}
