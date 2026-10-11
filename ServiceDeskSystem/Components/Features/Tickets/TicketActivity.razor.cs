using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Components.Features.Tickets;

/// <summary>
/// A comment written in the composer.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record NewTicketComment(string Message, bool IsInternal);

/// <summary>
/// New text of an existing comment.
/// </summary>
public sealed record TicketCommentEdit(int CommentId, string Message);
#pragma warning restore SA1313

/// <summary>
/// The ticket's activity: creation, status changes and comments in one timeline, followed by
/// the comment composer. The page saves through the callbacks.
/// </summary>
public partial class TicketActivity : BaseComponent
{
    private const string PublicVisibility = "public";
    private const string InternalVisibility = "internal";
    private const string SystemRowClass = "grid grid-cols-[3.25rem_minmax(0,1fr)] gap-3 py-1.5 font-mono text-13 text-muted";
    private const string SmallGhostClass = "btn-ghost h-6 px-1.5 font-mono text-13";

    private string newMessage = string.Empty;
    private string visibility = PublicVisibility;
    private bool sending;
    private int? editingId;
    private bool savingEdit;
    private string editingMessage = string.Empty;

    [Parameter]
    [EditorRequired]
    public Ticket Ticket { get; set; } = null!;

    [Parameter]
    public IReadOnlyList<TicketStatusHistory> History { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether the viewer is an admin or developer (sees and writes internal comments).
    /// </summary>
    [Parameter]
    public bool IsStaff { get; set; }

    [Parameter]
    public bool IsAdmin { get; set; }

    [Parameter]
    public int CurrentUserId { get; set; }

    /// <summary>
    /// Gets or sets the post of a new comment; returns <c>false</c> when it failed (the draft is kept).
    /// </summary>
    [Parameter]
    public Func<NewTicketComment, Task<bool>>? AddComment { get; set; }

    /// <summary>
    /// Gets or sets the save of an edited comment; returns <c>false</c> when it failed (the editor stays open).
    /// </summary>
    [Parameter]
    public Func<TicketCommentEdit, Task<bool>>? EditComment { get; set; }

    [Parameter]
    public EventCallback<int> OnDeleteComment { get; set; }

    private IReadOnlyList<TicketActivityEvent> Feed => TicketActivityFeed.Build(this.Ticket, this.History, this.IsStaff);

    private IReadOnlyList<(string Value, string Text)> VisibilityOptions =>
    [
        (PublicVisibility, this.Heading("details.visibilityPublic")),
        (InternalVisibility, this.Heading("details.visibilityInternal")),
    ];

    private static RenderFragment Actor(string? login) => builder =>
    {
        // Rows backfilled from the audit log have no actor.
        if (string.IsNullOrEmpty(login))
        {
            return;
        }

        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "text-fg");
        builder.AddContent(2, "@" + login);
        builder.CloseElement();
        builder.AddContent(3, " ");
    };

    /// <summary>
    /// Only the author edits a comment; admins can delete any comment.
    /// </summary>
    private bool CanEdit(Comment comment) => comment.AuthorId == this.CurrentUserId;

    private bool CanDelete(Comment comment) => comment.AuthorId == this.CurrentUserId || this.IsAdmin;

    private void SetVisibility(string value) => this.visibility = value;

    private async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(this.newMessage) || this.sending || this.AddComment is null)
        {
            return;
        }

        this.sending = true;
        try
        {
            if (await this.AddComment(new NewTicketComment(this.newMessage, this.IsStaff && this.visibility == InternalVisibility)))
            {
                this.newMessage = string.Empty;
                this.visibility = PublicVisibility;
            }
        }
        finally
        {
            this.sending = false;
        }
    }

    private void BeginEdit(Comment comment)
    {
        this.editingId = comment.Id;
        this.editingMessage = comment.Message;
    }

    private void CancelEdit()
    {
        this.editingId = null;
        this.editingMessage = string.Empty;
    }

    private async Task SaveEditAsync()
    {
        if (this.editingId is not { } id || string.IsNullOrWhiteSpace(this.editingMessage) || this.savingEdit || this.EditComment is null)
        {
            return;
        }

        this.savingEdit = true;
        try
        {
            if (await this.EditComment(new TicketCommentEdit(id, this.editingMessage.Trim())))
            {
                this.CancelEdit();
            }
        }
        finally
        {
            this.savingEdit = false;
        }
    }
}
