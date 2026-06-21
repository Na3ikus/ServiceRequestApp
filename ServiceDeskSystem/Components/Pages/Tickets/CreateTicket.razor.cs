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
    private ITicketService TicketService { get; set; } = null!;

    private TicketCreateModel ticketModel { get; set; } = new TicketCreateModel();

    private List<Product> products { get; set; } = [];

    private bool isSubmitting { get; set; }

    private bool isTypeSelected { get; set; }

    private string? productValidationError { get; set; }

    private bool IsProductRequired => this.ticketModel.TicketType != TicketType.Project;

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected override async Task OnInitializedAsync()
    {
        this.products = await this.TicketService.GetProductsAsync();
        this.ticketModel.Priority = TicketPriority.Medium;
    }

    private async Task HandleSubmitAsync()
    {
        this.productValidationError = null;

        if (this.IsProductRequired && !this.ticketModel.ProductId.HasValue)
        {
            this.productValidationError = "Please select a product";
            return;
        }

        this.isSubmitting = true;

        var role = this.AuthService.CurrentUser?.Role;
        bool isDevOrAdmin = role is UserRole.Admin or UserRole.Developer;

        var priority = isDevOrAdmin ? this.ticketModel.Priority : TicketPriority.Medium;
        bool isPriorityAssessed = isDevOrAdmin;

        var ticket = Ticket.Create(
            this.ticketModel.Title,
            this.ticketModel.Description,
            this.ticketModel.TicketType,
            priority,
            this.CurrentUserId,
            this.ticketModel.ProductId,
            isPriorityAssessed);

        ticket.StepsToReproduce = this.ticketModel.StepsToReproduce ?? string.Empty;
        ticket.Environment = this.ticketModel.Environment ?? string.Empty;
        ticket.AffectedVersion = this.ticketModel.AffectedVersion ?? string.Empty;

        await this.TicketService.CreateTicketAsync(ticket);

        this.Navigation.NavigateTo("/");
    }

    private void Cancel()
    {
        this.Navigation.NavigateTo("/");
    }

    private void SelectType(TicketType type)
    {
        this.ticketModel.TicketType = type;
        this.isTypeSelected = true;
        this.OnTicketTypeChanged();
    }

    private void BackToTypeSelection()
    {
        this.isTypeSelected = false;
    }

    private void OnTicketTypeChanged()
    {
        this.productValidationError = null;

        if (!this.IsProductRequired)
        {
            this.ticketModel.ProductId = null;
        }
    }

    private sealed class TicketCreateModel
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
