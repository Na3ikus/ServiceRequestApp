using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.Features.Tickets;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using PathSegment = ServiceDeskSystem.Components.UI.Primitives.PathSegment;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket details page: loads the ticket and wires the header actions, the properties panel
/// and the activity feed to the services.
/// </summary>
public partial class TicketDetails : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(30);
    private Timer? refreshTimer;
    private bool isRefreshing;
    private bool authRestored;

    [Parameter]
    public int Id { get; set; }

    [Inject]
    protected ITicketService TicketService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    [Inject]
    protected ITicketAssignmentService TicketAssignmentService { get; set; } = null!;

    [Inject]
    protected ICommentService CommentService { get; set; } = null!;

    [Inject]
    protected ITagService TagService { get; set; } = null!;

    protected Ticket? Ticket { get; set; }

    protected bool IsLoaded { get; set; }

    protected IReadOnlyList<TicketStatusHistory> History { get; set; } = [];

    protected IReadOnlyList<Tag> AllTags { get; set; } = [];

    protected string AnalyticalNoteEdit { get; set; } = string.Empty;

    protected bool IsEditingAnalyticalNote { get; set; }

    protected bool IsSavingAnalyticalNote { get; set; }

    protected bool IsBusy { get; set; }

    protected bool IsDeleteConfirmOpen { get; set; }

    protected bool IsDeleting { get; set; }

    protected string? DeleteError { get; set; }

    protected int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    protected UserRole? CurrentUserRole => this.AuthService.CurrentUser?.Role;

    protected bool IsAdmin => this.CurrentUserRole == UserRole.Admin;

    protected bool CanManageTicket => this.Ticket is not null && this.AuthService.IsAuthenticated && (this.Ticket.AuthorId == this.CurrentUserId || this.IsAdmin);

    /// <summary>
    /// Gets a value indicating whether the user can manage ticket status: ONLY the person who took the ticket (DeveloperId == CurrentUserId).
    /// Admin cannot manage status unless they took the ticket themselves.
    /// </summary>
    protected bool CanManageTicketStatus => this.AuthService.IsAuthenticated &&
        this.Ticket?.DeveloperId == this.CurrentUserId;

    /// <summary>
    /// Gets a value indicating whether "take" is offered: the ticket is free and the user is an admin or developer.
    /// </summary>
    protected bool ShowTakeButton => this.Ticket?.DeveloperId is null && this.IsAdminOrDeveloper;

    /// <summary>
    /// Gets a value indicating whether "release" is offered: the ticket is assigned to the current user (admins release only their own).
    /// </summary>
    protected bool ShowReleaseButton => this.Ticket?.DeveloperId is not null && this.Ticket.DeveloperId == this.CurrentUserId;

    /// <summary>
    /// Gets the role's ticket list: <c>/my-tickets</c> for users, <c>/tickets</c> for staff.
    /// </summary>
    protected string ListHref => this.IsAdminOrDeveloper ? "/tickets" : "/my-tickets";

    protected string ListTitleKey => this.IsAdminOrDeveloper ? "nav.allTickets" : "nav.myTickets";

    protected IReadOnlyList<PathSegment> Segments =>
    [
        new ("tickets", this.ListHref),
        new (this.Id.ToString(System.Globalization.CultureInfo.InvariantCulture)),
    ];

    protected override async Task OnInitializedAsync()
    {
        this.AuthService.AuthStateChanged += this.OnAuthStateChanged;

        await this.LoadTicketAsync(isInitialLoad: true);
        this.StartAutoRefresh();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !this.authRestored)
        {
            await this.AuthService.EnsureRestoredAsync();
            this.authRestored = true;
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (this.disposed)
        {
            return;
        }

        if (disposing)
        {
            this.AuthService.AuthStateChanged -= this.OnAuthStateChanged;
            this.refreshTimer?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected void GoToList() => this.Navigation.NavigateTo(this.ListHref);

    protected async Task SavePriorityAsync(TicketPriority priority)
    {
        if (this.Ticket is null || !this.IsAdminOrDeveloper)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketPriorityAsync(this.Ticket.Id, priority);
        if (success)
        {
            await this.LoadTicketAsync();
            await this.ToastService.ShowToastAsync(this.L.Translate("details.priorityUpdated"), ToastType.Success);
        }
    }

    protected async Task SaveDatesAsync(TicketDatesChange change)
    {
        ArgumentNullException.ThrowIfNull(change);

        if (this.Ticket is null || !this.IsAdminOrDeveloper)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketDatesAsync(this.Ticket.Id, change.StartUtc, change.DueUtc, this.CurrentUserId);
        if (success)
        {
            this.Ticket.StartDate = change.StartUtc;
            this.Ticket.DueDate = change.DueUtc;
            await this.ToastService.ShowToastAsync(this.L.Translate("details.datesUpdated"), ToastType.Success);
        }
    }

    protected async Task AddCommentAsync(NewTicketComment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        if (string.IsNullOrWhiteSpace(comment.Message) || this.Ticket is null)
        {
            return;
        }

        await this.CommentService.AddCommentAsync(new Comment
        {
            Message = comment.Message,
            TicketId = this.Ticket.Id,
            AuthorId = this.CurrentUserId,
            IsInternal = this.IsAdminOrDeveloper && comment.IsInternal,
        });

        await this.LoadTicketAsync();
    }

    protected async Task SaveCommentEditAsync(TicketCommentEdit edit)
    {
        ArgumentNullException.ThrowIfNull(edit);

        var comment = this.Ticket?.Comments.FirstOrDefault(c => c.Id == edit.CommentId);
        if (comment is null || comment.AuthorId != this.CurrentUserId || string.IsNullOrWhiteSpace(edit.Message))
        {
            return;
        }

        var updated = await this.CommentService.UpdateCommentAsync(edit.CommentId, edit.Message, this.CurrentUserId, this.IsAdmin);
        if (updated is not null)
        {
            comment.Message = updated.Message;
        }

        await this.LoadTicketAsync();
    }

    protected async Task DeleteCommentAsync(int commentId)
    {
        var comment = this.Ticket?.Comments.FirstOrDefault(c => c.Id == commentId);
        if (comment is null || !(comment.AuthorId == this.CurrentUserId || this.IsAdmin))
        {
            return;
        }

        var success = await this.CommentService.DeleteCommentAsync(commentId);
        if (success)
        {
            this.Ticket!.Comments.Remove(comment);
        }

        await this.LoadTicketAsync();
    }

    protected async Task UpdateStatusAsync(TicketStatus newStatus)
    {
        if (this.Ticket is null || !this.CanManageTicketStatus)
        {
            return;
        }

        this.IsBusy = true;
        try
        {
            var success = await this.TicketService.UpdateTicketStatusAsync(this.Ticket.Id, newStatus, this.AuthService.CurrentUser?.Id);
            if (success)
            {
                this.Ticket.Status = newStatus;
                await this.LoadTicketAsync();
            }
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    protected void OpenDeleteConfirm()
    {
        this.DeleteError = null;
        this.IsDeleteConfirmOpen = true;
    }

    protected void CloseDeleteConfirm()
    {
        if (!this.IsDeleting)
        {
            this.IsDeleteConfirmOpen = false;
        }
    }

    protected async Task DeleteTicketAsync()
    {
        if (this.Ticket is null || !this.CanManageTicket)
        {
            return;
        }

        this.IsDeleting = true;
        try
        {
            var success = await this.TicketService.DeleteTicketAsync(this.Ticket.Id);
            if (success)
            {
                this.IsDeleteConfirmOpen = false;
                this.Navigation.NavigateTo(this.ListHref);
                return;
            }

            this.DeleteError = this.L.Translate("details.deleteFailed");
        }
        finally
        {
            this.IsDeleting = false;
        }
    }

    protected async Task AssignToMeAsync()
    {
        if (this.Ticket is null)
        {
            return;
        }

        if (this.Ticket.AuthorId == this.CurrentUserId)
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("details.cannotTakeOwnTicket"), ToastType.Warning);
            return;
        }

        if (this.Ticket.DeveloperId is not null)
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("details.ticketAlreadyAssigned"), ToastType.Warning);
            await this.LoadTicketAsync();
            return;
        }

        this.IsBusy = true;
        try
        {
            if (await this.TicketAssignmentService.AssignDeveloperAsync(this.Ticket.Id, this.CurrentUserId))
            {
                await this.LoadTicketAsync();
            }
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    protected async Task UnassignAsync()
    {
        if (this.Ticket is null)
        {
            return;
        }

        this.IsBusy = true;
        try
        {
            if (await this.TicketAssignmentService.UnassignDeveloperAsync(this.Ticket.Id))
            {
                await this.LoadTicketAsync();
            }
        }
        finally
        {
            this.IsBusy = false;
        }
    }

    protected async Task AssignTagToTicketAsync(int tagId)
    {
        if (this.Ticket is null || tagId <= 0)
        {
            return;
        }

        var success = await this.TagService.AssignTagToTicketAsync(this.Ticket.Id, tagId, this.CurrentUserId);
        if (success)
        {
            await this.LoadTicketAsync();
            await this.ToastService.ShowToastAsync(this.L.Translate("tags.assigned"), ToastType.Success);
        }
    }

    protected async Task RemoveTagFromTicketAsync(int tagId)
    {
        if (this.Ticket is null)
        {
            return;
        }

        var success = await this.TagService.RemoveTagFromTicketAsync(this.Ticket.Id, tagId, this.CurrentUserId);
        if (success)
        {
            await this.LoadTicketAsync();
            await this.ToastService.ShowToastAsync(this.L.Translate("tags.removed"), ToastType.Success);
        }
    }

    protected void BeginEditAnalyticalNote()
    {
        this.AnalyticalNoteEdit = this.Ticket?.AnalyticalNote ?? string.Empty;
        this.IsEditingAnalyticalNote = true;
    }

    protected void CancelEditAnalyticalNote()
    {
        this.AnalyticalNoteEdit = this.Ticket?.AnalyticalNote ?? string.Empty;
        this.IsEditingAnalyticalNote = false;
    }

    protected async Task SaveAnalyticalNoteAsync()
    {
        if (this.Ticket is null || !this.IsAdminOrDeveloper)
        {
            return;
        }

        this.IsSavingAnalyticalNote = true;
        try
        {
            var note = this.AnalyticalNoteEdit.Trim();
            var success = await this.TicketService.UpdateAnalyticalNoteAsync(this.Ticket.Id, note, this.CurrentUserId);
            if (success)
            {
                this.Ticket.AnalyticalNote = note;
                this.IsEditingAnalyticalNote = false;
                await this.ToastService.ShowToastAsync(this.L.Translate("details.analyticalNoteSaved"), ToastType.Success);
            }
        }
        finally
        {
            this.IsSavingAnalyticalNote = false;
        }
    }

    private async Task LoadTicketAsync(bool isInitialLoad = false)
    {
        var fetchedTicket = await this.TicketService.GetTicketByIdAsync(this.Id);
        if (fetchedTicket is not null)
        {
            this.Ticket = fetchedTicket;
            this.History = await this.TicketService.GetStatusHistoryAsync(this.Id);

            // Keep an analytical note that is being edited; the panel keeps its own unsaved edits.
            if (isInitialLoad || !this.IsEditingAnalyticalNote)
            {
                this.AnalyticalNoteEdit = fetchedTicket.AnalyticalNote ?? string.Empty;
            }

            if (isInitialLoad || this.AllTags.Count == 0)
            {
                this.AllTags = (await this.TagService.GetAllTagsAsync()).ToList();
            }
        }
        else if (isInitialLoad)
        {
            this.Ticket = null;
        }

        this.IsLoaded = true;
        this.StateHasChanged();
    }

    private void StartAutoRefresh()
    {
        this.refreshTimer = new Timer(async _ => await this.RefreshAsync(), null, this.refreshInterval, this.refreshInterval);
    }

    private async Task RefreshAsync()
    {
        if (this.isRefreshing || this.Ticket is null)
        {
            return;
        }

        this.isRefreshing = true;
        try
        {
            await this.InvokeAsync(() => this.LoadTicketAsync());
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        _ = this.InvokeAsync(this.StateHasChanged);
    }
}
