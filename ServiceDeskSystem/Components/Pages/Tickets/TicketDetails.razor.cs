using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Components.Common;
using ServiceDeskSystem.Data.Entities;
using ServiceDeskSystem.Services.Auth;
using ServiceDeskSystem.Services.Localization;
using ServiceDeskSystem.Services.Tickets;

namespace ServiceDeskSystem.Components.Pages.Tickets;

/// <summary>
/// Ticket details page component.
/// </summary>
public partial class TicketDetails : BaseComponent
{
    private readonly TimeSpan refreshInterval = TimeSpan.FromSeconds(5);
    private readonly List<ToastMessage> toasts = [];
    private Timer? refreshTimer;
    private bool isRefreshing;
    private bool authRestored;

    internal IReadOnlyList<ToastMessage> Toasts => this.toasts;

    [Parameter]
    public int Id { get; set; }

    [Inject]
    private ITicketService TicketService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private NavigationManager Navigation { get; set; } = null!;

    private Ticket? Ticket { get; set; }

    private string NewCommentMessage { get; set; } = string.Empty;

    private bool IsSubmitting { get; set; }

    private int? EditingCommentId { get; set; }

    private string EditingCommentMessage { get; set; } = string.Empty;

    private int CurrentUserId => this.AuthService.CurrentUser?.Id ?? 0;

    private string CurrentUserRole => this.AuthService.CurrentUser?.Role ?? string.Empty;

    private bool IsAdmin => string.Equals(this.CurrentUserRole, "Admin", StringComparison.OrdinalIgnoreCase);

    private bool IsDeveloper => string.Equals(this.CurrentUserRole, "Developer", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Combined check: user is admin or developer (both can take/release tickets).
    /// </summary>
    private bool IsAdminOrDeveloper => this.AuthService.IsAuthenticated && (this.IsAdmin || this.IsDeveloper);

    private bool CanManageTicket => this.Ticket is not null && this.AuthService.IsAuthenticated && (this.Ticket.AuthorId == this.CurrentUserId || this.IsAdmin);

    /// <summary>
    /// Can manage ticket status: ONLY the person who took the ticket (DeveloperId == CurrentUserId).
    /// Admin cannot manage status unless they took the ticket themselves.
    /// </summary>
    private bool CanManageTicketStatus => this.AuthService.IsAuthenticated &&
        this.Ticket?.DeveloperId == this.CurrentUserId;

    private bool CanDeleteComment(Comment comment) => comment.AuthorId == this.CurrentUserId || this.IsAdmin;

    internal async Task RemoveToastAsync(ToastMessage toast)
    {
        toast.IsHiding = true;
        await this.InvokeAsync(this.StateHasChanged);
        await Task.Delay(300);
        this.toasts.Remove(toast);
        await this.InvokeAsync(this.StateHasChanged);
    }

    protected override async Task OnInitializedAsync()
    {
        this.AuthService.AuthStateChanged += this.OnAuthStateChanged;

        await this.LoadTicketAsync();
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

    private static string GetStatusBadgeClass(string status) => status switch
    {
        "Open" => "bg-blue-100 text-blue-800",
        "In Progress" => "bg-yellow-100 text-yellow-800",
        "Resolved" => "bg-green-100 text-green-800",
        "Closed" => "bg-gray-100 text-gray-800",
        _ => "bg-gray-100 text-gray-800",
    };

    private static string GetPriorityBadgeClass(string priority) => priority switch
    {
        "Critical" => "bg-red-100 text-red-800",
        "High" => "bg-orange-100 text-orange-800",
        "Medium" => "bg-yellow-100 text-yellow-800",
        "Low" => "bg-green-100 text-green-800",
        _ => "bg-gray-100 text-gray-800",
    };

    private async Task LoadTicketAsync()
    {
        this.Ticket = await this.TicketService.GetTicketByIdAsync(this.Id);
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

    private async Task AddCommentAsync()
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
        };

        var addedComment = await this.TicketService.AddCommentAsync(comment);
        this.Ticket.Comments.Add(addedComment);

        this.NewCommentMessage = string.Empty;
        this.IsSubmitting = false;

        await this.RefreshCommentsAsync();
    }

    private void BeginEdit(Comment comment)
    {
        this.EditingCommentId = comment.Id;
        this.EditingCommentMessage = comment.Message;
    }

    private void CancelEdit()
    {
        this.EditingCommentId = null;
        this.EditingCommentMessage = string.Empty;
    }

    private async Task SaveCommentEditAsync(int commentId)
    {
        if (this.EditingCommentId != commentId || string.IsNullOrWhiteSpace(this.EditingCommentMessage))
        {
            return;
        }

        var comment = this.Ticket?.Comments?.FirstOrDefault(c => c.Id == commentId);

        // Тільки автор може редагувати коментар (адмін НЕ може редагувати чужі)
        if (comment is null || comment.AuthorId != this.CurrentUserId)
        {
            return;
        }

        var updated = await this.TicketService.UpdateCommentAsync(commentId, this.EditingCommentMessage.Trim());
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

    /// <summary>
    /// Тільки автор може редагувати свій коментар. Адмін НЕ може редагувати чужі.
    /// </summary>
    private bool CanEditComment(Comment comment) => comment.AuthorId == this.CurrentUserId;

    private async Task DeleteCommentAsync(int commentId)
    {
        var comment = this.Ticket?.Comments?.FirstOrDefault(c => c.Id == commentId);
        if (comment is null || !this.CanDeleteComment(comment))
        {
            return;
        }

        var success = await this.TicketService.DeleteCommentAsync(commentId);
        if (success && this.Ticket?.Comments is not null)
        {
            this.Ticket.Comments.Remove(comment);
        }

        await this.RefreshCommentsAsync();
    }

    private async Task UpdateStatusAsync(string newStatus)
    {
        if (this.Ticket is null || !this.CanManageTicketStatus)
        {
            return;
        }

        var success = await this.TicketService.UpdateTicketStatusAsync(this.Ticket.Id, newStatus);
        if (success)
        {
            this.Ticket.Status = newStatus;
        }
    }

    private async Task DeleteTicketAsync()
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

    private void GoBack()
    {
        this.Navigation.NavigateTo("/");
    }

    private async Task AssignToMeAsync()
    {
        if (this.Ticket is null)
        {
            return;
        }

        // Перевірка: автор не може взяти свій тікет
        if (this.Ticket.AuthorId == this.CurrentUserId)
        {
            await this.ShowToastAsync(this.L.Translate("details.cannotTakeOwnTicket"), ToastType.Warning);
            return;
        }

        // Перевірка: тікет вже призначений комусь іншому
        if (this.Ticket.DeveloperId is not null)
        {
            await this.ShowToastAsync(this.L.Translate("details.ticketAlreadyAssigned"), ToastType.Warning);
            await this.LoadTicketAsync();
            return;
        }

        var success = await this.TicketService.AssignDeveloperAsync(this.Ticket.Id, this.CurrentUserId);
        if (success)
        {
            await this.LoadTicketAsync();
        }
    }

    private async Task UnassignAsync()
    {
        if (this.Ticket is null)
        {
            return;
        }

        var success = await this.TicketService.UnassignDeveloperAsync(this.Ticket.Id);
        if (success)
        {
            await this.LoadTicketAsync();
        }
    }

    private async Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 4000)
    {
        var toast = new ToastMessage { Message = message, Type = type };
        this.toasts.Add(toast);
        await this.InvokeAsync(this.StateHasChanged);

        _ = Task.Run(async () =>
        {
            await Task.Delay(durationMs);

            toast.IsHiding = true;
            await this.InvokeAsync(this.StateHasChanged);

            await Task.Delay(300);
            this.toasts.Remove(toast);
            await this.InvokeAsync(this.StateHasChanged);
        });
    }

    private void OnAuthStateChanged(object? sender, EventArgs e)
    {
        _ = this.InvokeAsync(this.StateHasChanged);
    }
}
