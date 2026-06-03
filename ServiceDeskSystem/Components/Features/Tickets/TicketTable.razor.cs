using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// Displays a table of tickets with configurable columns.
/// </summary>
public partial class TicketTable
{
    [Parameter]
    public IEnumerable<Ticket> Tickets { get; set; } = [];

    [Parameter]
    public EventCallback<int> OnTicketClick { get; set; }

    [Parameter]
    public bool ShowProduct { get; set; } = true;

    [Parameter]
    public bool ShowAuthor { get; set; } = true;

    [Parameter]
    public bool ShowAssignee { get; set; } = true;

    private static string GetPriorityColorClass(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => "bg-gradient-to-br from-red-500 to-rose-600",
        TicketPriority.High => "bg-gradient-to-br from-orange-400 to-red-500",
        TicketPriority.Medium => "bg-gradient-to-br from-amber-400 to-orange-500",
        TicketPriority.Low => "bg-gradient-to-br from-green-400 to-emerald-500",
        _ => "bg-gradient-to-br from-gray-400 to-gray-500",
    };

    private static string GetStatusDotColor(TicketStatus status) => status switch
    {
        TicketStatus.Open => "bg-blue-500",
        TicketStatus.InProgress => "bg-amber-500",
        TicketStatus.Testing => "bg-cyan-500",
        TicketStatus.CodeReview => "bg-indigo-500",
        TicketStatus.Resolved => "bg-green-500",
        TicketStatus.Closed => "bg-gray-500",
        _ => "bg-purple-500",
    };
}
