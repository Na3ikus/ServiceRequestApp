using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Tickets;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Create ticket page component.
/// </summary>
public partial class CreateTicket
{
    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private TicketCreateModel ticketModel { get; set; } = new ();

    private List<Product> products { get; set; } = [];

    private bool isSubmitting { get; set; }

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected override async Task OnInitializedAsync()
    {
        this.products = await this.TicketService.GetProductsAsync();
    }

    private async Task HandleSubmitAsync()
    {
        if (this.ticketModel.ProductId == 0)
        {
            return;
        }

        this.isSubmitting = true;

        var ticket = new Ticket
        {
            Title = this.ticketModel.Title,
            Description = this.ticketModel.Description,
            Priority = this.ticketModel.Priority,
            ProductId = this.ticketModel.ProductId,
            StepsToReproduce = this.ticketModel.StepsToReproduce ?? string.Empty,
            Environment = this.ticketModel.Environment ?? string.Empty,
            AffectedVersion = string.Empty,
            AuthorId = this.CurrentUserId,
        };

        await this.TicketService.CreateTicketAsync(ticket);

        this.Navigation.NavigateTo("/");
    }

    private void Cancel()
    {
        this.Navigation.NavigateTo("/");
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
        public string Priority { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Please select a product")]
        public int ProductId { get; set; }

        public string? StepsToReproduce { get; set; }

        public string? Environment { get; set; }
    }
}
