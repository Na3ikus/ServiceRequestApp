using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Api.Models;

public sealed record CreateTicketDto(
    string Title,
    string Description,
    TicketPriority Priority,
    TicketType Type,
    int? ProductId
);
