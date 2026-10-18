using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Infrastructure.Data.DataSeeding.Demo;

/// <summary>
/// A hand-written demo ticket. <see cref="DemoDataSeeder"/> turns it into a ticket with history, SLA, comments,
/// work logs, notifications and audit entries, dated relative to the moment it seeds.
/// </summary>
internal sealed record DemoTicket(
    string Title,
    string Description,
    string Steps,
    int ProductId,
    TicketType Type,
    TicketPriority Priority,
    int AuthorId,
    int? DeveloperId,
    TicketStatus Status,
    double AgeDays,
    string Environment,
    string Version,
    string[] Tags)
{
    /// <summary>Gets the developer's analytical note, if any.</summary>
    public string? Note { get; init; }

    /// <summary>Gets a value indicating whether staff have assessed the client's priority.</summary>
    public bool PriorityAssessed { get; init; } = true;

    /// <summary>Gets the planned due date of open work, in days from the seed; otherwise the SLA due date applies.</summary>
    public int? DueInDays { get; init; }

    /// <summary>
    /// Gets a scripted thread used instead of generated comments. Each line starts with who speaks:
    /// <c>c:</c> the author, <c>d:</c> the developer, <c>i:</c> the developer internally, <c>a:</c> the admin.
    /// </summary>
    public string[] Conversation { get; init; } = [];

    /// <summary>Gets a value indicating whether the ticket is written in Ukrainian (comments and work logs follow it).</summary>
    public bool IsUkrainian => this.Title.Any(c => c is >= 'А' and <= 'я' or 'і' or 'ї' or 'є' or 'ґ' or 'І' or 'Ї' or 'Є' or 'Ґ');
}
