using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin users section: role and active state change in the row; users register through /register.
/// </summary>
public partial class UsersTab : BaseComponent
{
    // Role identifiers stay English, like the section names (Stage 3 decision 5).
    private static readonly IReadOnlyList<(string Value, string Text)> RoleOptions =
    [
        (nameof(UserRole.Admin), "admin"),
        (nameof(UserRole.Developer), "dev"),
        (nameof(UserRole.User), "user"),
    ];

    private int? failedRoleUserId;

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<User>? Users { get; set; }

    [Parameter]
    public EventCallback OnUsersChanged { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected RowState Row { get; } = new ();

    protected string SearchQuery { get; set; } = string.Empty;

    protected static string GetDisplayName(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var fullName = $"{user.Person?.FirstName} {user.Person?.LastName}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.Login : fullName;
    }

    protected static bool CanDeleteUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.Role != UserRole.Admin;
    }

    protected static bool CanToggleUserStatus(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return CanDeleteUser(user);
    }

    protected IList<User>? GetFilteredUsers() =>
        this.Users?
            .Where(u =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                u.Login.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (u.Person?.FirstName ?? string.Empty).Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (u.Person?.LastName ?? string.Empty).Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.Person?.LastName ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ToList();

    protected void ClearSearch() => this.SearchQuery = string.Empty;

    protected bool CanEditUserRole(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.Id != this.AuthService.CurrentUser?.Id;
    }

    /// <summary>
    /// Saves a new role; the control shows the stored role, so a failed save leaves the previous one.
    /// </summary>
    /// <param name="user">The user whose role changes.</param>
    /// <param name="value">The <see cref="UserRole"/> name picked in the control.</param>
    /// <returns>A task that completes when the save and the follow-up refresh finish.</returns>
    protected async Task ChangeRoleAsync(User user, string value)
    {
        ArgumentNullException.ThrowIfNull(user);

        this.failedRoleUserId = null;
        if (!Enum.TryParse<UserRole>(value, out var newRole) || newRole == user.Role)
        {
            return;
        }

        if (!this.CanEditUserRole(user))
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotEditSelfRole"), ToastType.Error);
            return;
        }

        bool success;
        try
        {
            success = await this.AdminService.UpdateUserRoleAsync(user.Id, newRole);
        }
        catch (Exception)
        {
            success = false;
        }

        if (!success)
        {
            this.failedRoleUserId = user.Id;
            return;
        }

        user.Role = newRole;
        await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
        await this.OnUsersChanged.InvokeAsync();
    }

    protected async Task ToggleUserStatusAsync(int userId)
    {
        var user = this.Users?.FirstOrDefault(u => u.Id == userId);
        if (user?.Role == UserRole.Admin)
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeactivateAdmin"), ToastType.Error);
            return;
        }

        try
        {
            var success = await this.AdminService.ToggleUserActiveStatusAsync(userId);
            if (success && user is not null)
            {
                user.IsActive = !user.IsActive;
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
                await this.OnUsersChanged.InvokeAsync();
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error updating user: {ex.Message}", ToastType.Error);
        }
    }

    protected async Task DeleteUserAsync(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (user.Role == UserRole.Admin)
        {
            this.Row.Close();
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteAdmin"), ToastType.Error);
            return;
        }

        this.Row.Begin();
        try
        {
            var success = await this.AdminService.DeleteUserAsync(user.Id);
            this.Row.Close();
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.userDeleted"), ToastType.Success);
                this.Users?.Remove(user);
                await this.OnUsersChanged.InvokeAsync();
            }
            else
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteUserWithTickets"), ToastType.Error);
            }
        }
        catch (Exception ex)
        {
            this.Row.Close();
            await this.ToastService.ShowToastAsync($"Error deleting user: {ex.Message}", ToastType.Error);
        }
    }
}
