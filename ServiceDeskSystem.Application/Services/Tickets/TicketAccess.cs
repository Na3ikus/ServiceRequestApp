using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Tickets;

/// <summary>
/// Who may open a ticket: admins and developers see every ticket, a user only the tickets they wrote.
/// Callers show a ticket that fails the check as missing, so ticket numbers cannot be probed.
/// </summary>
public static class TicketAccess
{
    public static bool CanView(Ticket ticket, User? user) =>
        user is not null && CanView(ticket, user.Id, user.Role);

    public static bool CanView(Ticket ticket, int? userId, UserRole? role)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        return role is UserRole.Admin or UserRole.Developer
            || (role is UserRole.User && userId is not null && ticket.AuthorId == userId);
    }
}
