using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Common;

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
}
