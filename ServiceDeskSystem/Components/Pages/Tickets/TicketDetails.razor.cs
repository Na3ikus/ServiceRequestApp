using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Comments;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Tickets;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket details page component.
/// </summary>
public partial class TicketDetails : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
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

    protected string NewCommentMessage { get; set; } = string.Empty;

    protected bool IsInternalComment { get; set; }

    protected bool IsSubmitting { get; set; }

    protected int? EditingCommentId { get; set; }

    protected string EditingCommentMessage { get; set; } = string.Empty;

    protected DateTime? EditStartDate { get; set; }

    protected DateTime? EditDueDate { get; set; }

    protected TicketPriority EditPriority { get; set; }

    protected bool IsCriticalPriorityConfirmed { get; set; }

    protected IList<Tag> AllTags { get; set; } = [];

    protected int SelectedTagIdToAdd { get; set; }

    protected string AnalyticalNoteEdit { get; set; } = string.Empty;

    protected bool IsEditingAnalyticalNote { get; set; }

    protected bool IsSavingAnalyticalNote { get; set; }

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

    protected bool CanDeleteComment(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        return comment.AuthorId == this.CurrentUserId || this.IsAdmin;
    }

    /// <summary>
    /// Only author can edit comment. Admin cannot edit other users' comments.
    /// </summary>
    protected bool CanEditComment(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);
        return comment.AuthorId == this.CurrentUserId;
    }

    protected void OnPriorityChanged()
    {
        this.IsCriticalPriorityConfirmed = false;
    }

    protected async Task OnPriorityChangedAsync(ChangeEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(e);

        if (Enum.TryParse<TicketPriority>(e.Value?.ToString(), out var priority))
        {
            this.EditPriority = priority;
            this.IsCriticalPriorityConfirmed = false;
        }

        await Task.CompletedTask;
    }

    protected async Task SavePriorityAsync()
    {
        if (this.Ticket is null || !this.IsAdminOrDeveloper)
        {
            return;
        }

        if (this.EditPriority == TicketPriority.Critical && !this.IsCriticalPriorityConfirmed)
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("create.criticalRequired") ?? "Please confirm critical priority.", ToastType.Error);
            return;
        }

        var success = await this.TicketService.UpdateTicketPriorityAsync(this.Ticket.Id, this.EditPriority);
        if (success)
        {
            this.Ticket.Priority = this.EditPriority;
            this.Ticket.IsPriorityAssessed = true;
            await this.LoadTicketAsync(isInitialLoad: true);
            await this.ToastService.ShowToastAsync(this.L.Translate("details.priorityUpdated") ?? "Priority updated successfully.", ToastType.Success);
        }
    }

    protected async Task SaveDatesAsync()
    {
        if (this.Ticket is null || !this.IsAdminOrDeveloper)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketDatesAsync(this.Ticket.Id, this.EditStartDate, this.EditDueDate, this.CurrentUserId);
        if (success)
        {
            this.Ticket.StartDate = this.EditStartDate;
            this.Ticket.DueDate = this.EditDueDate;
            await this.ToastService.ShowToastAsync(this.L.Translate("details.datesUpdated") ?? "Dates updated successfully.", ToastType.Success);
        }
    }

    protected async Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(this.NewCommentMessage) || this.Ticket is null)
        {
            return;
        }

        this.IsSubmitting = true;

        var comment = new Comment
        {
            Message = this.NewCommentMessage,
            TicketId = this.Ticket.Id,
            AuthorId = this.CurrentUserId,
            IsInternal = this.IsAdminOrDeveloper && this.IsInternalComment,
        };

        var addedComment = await this.CommentService.AddCommentAsync(comment);
        this.Ticket.Comments.Add(addedComment);

        this.NewCommentMessage = string.Empty;
        this.IsInternalComment = false;
        this.IsSubmitting = false;

        await this.RefreshCommentsAsync();
    }

    protected void BeginEdit(Comment comment)
    {
        ArgumentNullException.ThrowIfNull(comment);

        this.EditingCommentId = comment.Id;
        this.EditingCommentMessage = comment.Message;
    }

    protected void CancelEdit()
    {
        this.EditingCommentId = null;
        this.EditingCommentMessage = string.Empty;
    }

    protected async Task SaveCommentEditAsync(int commentId)
    {
        if (this.EditingCommentId != commentId || string.IsNullOrWhiteSpace(this.EditingCommentMessage))
        {
            return;
        }

        var comment = this.Ticket?.Comments?.FirstOrDefault(c => c.Id == commentId);

        if (comment is null || comment.AuthorId != this.CurrentUserId)
        {
            return;
        }

        var updated = await this.CommentService.UpdateCommentAsync(commentId, this.EditingCommentMessage.Trim(), this.CurrentUserId, this.IsAdmin);
        if (updated is not null && this.Ticket?.Comments is not null)
        {
            var existing = this.Ticket.Comments.FirstOrDefault(c => c.Id == commentId);
            if (existing is not null)
            {
                existing.Message = updated.Message;
            }
        }

        this.CancelEdit();
        await this.RefreshCommentsAsync();
    }

    protected async Task DeleteCommentAsync(int commentId)
    {
        var comment = this.Ticket?.Comments?.FirstOrDefault(c => c.Id == commentId);
        if (comment is null || !this.CanDeleteComment(comment))
        {
            return;
        }

        var success = await this.CommentService.DeleteCommentAsync(commentId);
        if (success && this.Ticket?.Comments is not null)
        {
            this.Ticket.Comments.Remove(comment);
        }

        await this.RefreshCommentsAsync();
    }

    protected async Task UpdateStatusAsync(TicketStatus newStatus)
    {
        if (this.Ticket is null || !this.CanManageTicketStatus)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketStatusAsync(this.Ticket.Id, newStatus);
        if (success)
        {
            this.Ticket.Status = newStatus;
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    protected async Task DeleteTicketAsync()
    {
        if (this.Ticket is null || !this.CanManageTicket)
        {
            return;
        }

        var success = await this.TicketService.DeleteTicketAsync(this.Ticket.Id);
        if (success)
        {
            this.Navigation.NavigateTo("/");
        }
    }

    protected void GoBack()
    {
        this.Navigation.NavigateTo("/");
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

        var success = await this.TicketAssignmentService.AssignDeveloperAsync(this.Ticket.Id, this.CurrentUserId);
        if (success)
        {
            await this.LoadTicketAsync();
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    protected async Task UnassignAsync()
    {
        if (this.Ticket is null)
        {
            return;
        }

        var success = await this.TicketAssignmentService.UnassignDeveloperAsync(this.Ticket.Id);
        if (success)
        {
            await this.LoadTicketAsync();
            await this.InvokeAsync(this.StateHasChanged);
        }
    }

    protected async Task AssignTagToTicketAsync()
    {
        if (this.Ticket is null || this.SelectedTagIdToAdd <= 0)
        {
            return;
        }

        var success = await this.TagService.AssignTagToTicketAsync(this.Ticket.Id, this.SelectedTagIdToAdd, this.CurrentUserId);
        if (success)
        {
            this.SelectedTagIdToAdd = 0;
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
            var success = await this.TicketService.UpdateAnalyticalNoteAsync(this.Ticket.Id, this.AnalyticalNoteEdit.Trim(), this.CurrentUserId);
            if (success)
            {
                this.Ticket.AnalyticalNote = this.AnalyticalNoteEdit.Trim();
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
            var oldTicketPriority = this.Ticket?.Priority;
            var oldTicketStartDate = this.Ticket?.StartDate;
            var oldTicketDueDate = this.Ticket?.DueDate;

            this.Ticket = fetchedTicket;

            // Only overwrite edit form controls during initial load or if user hasn't modified them
            if (isInitialLoad || this.EditPriority == oldTicketPriority)
            {
                this.EditPriority = fetchedTicket.Priority;
            }

            if (isInitialLoad || this.EditStartDate == oldTicketStartDate)
            {
                this.EditStartDate = fetchedTicket.StartDate;
            }

            if (isInitialLoad || this.EditDueDate == oldTicketDueDate)
            {
                this.EditDueDate = fetchedTicket.DueDate;
            }

            if (isInitialLoad || !this.IsEditingAnalyticalNote)
            {
                this.AnalyticalNoteEdit = fetchedTicket.AnalyticalNote ?? string.Empty;
            }

            this.AllTags = (await this.TagService.GetAllTagsAsync()).ToList();
        }

        this.StateHasChanged();
    }

    private void StartAutoRefresh()
    {
        this.refreshTimer = new Timer(async _ => await this.RefreshCommentsAsync(), null, this.refreshInterval, this.refreshInterval);
    }

    private async Task RefreshCommentsAsync()
    {
        if (this.isRefreshing)
        {
            return;
        }

        this.isRefreshing = true;
        try
        {
            await this.InvokeAsync(async () =>
            {
                await this.LoadTicketAsync();
                this.StateHasChanged();
            });
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
