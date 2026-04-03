using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Domain.Entities;

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

    private static string GetPriorityColorClass(string priority) => priority switch
    {
        "Critical" => "bg-gradient-to-br from-red-500 to-rose-600",
        "High" => "bg-gradient-to-br from-orange-400 to-red-500",
        "Medium" => "bg-gradient-to-br from-amber-400 to-orange-500",
        "Low" => "bg-gradient-to-br from-green-400 to-emerald-500",
        _ => "bg-gradient-to-br from-gray-400 to-gray-500",
    };

    private static string GetStatusDotColor(string status) => status switch
    {
        "Open" => "bg-blue-500",
        "In Progress" => "bg-amber-500",
        "Testing" => "bg-cyan-500",
        "Code Review" => "bg-indigo-500",
        "Resolved" => "bg-green-500",
        "Closed" => "bg-gray-500",
        _ => "bg-purple-500",
    };
}
