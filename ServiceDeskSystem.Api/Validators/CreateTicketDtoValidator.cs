using FluentValidation;
using ServiceDeskSystem.Api.Models;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Api.Validators;

public class CreateTicketDtoValidator : AbstractValidator<CreateTicketDto>
{
    public CreateTicketDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty();
        // Validation for Project vs Non-Project
        RuleFor(x => x.ProductId).NotNull().When(x => x.Type != TicketType.Project)
            .WithMessage("Product is required for non-project tickets.");
    }
}
