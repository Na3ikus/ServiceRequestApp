using FluentValidation;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Api.Validators;

public class TicketValidator : AbstractValidator<Ticket>
{
    public TicketValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        // Validation for Project vs Non-Project is handled in the TicketService
    }
}
