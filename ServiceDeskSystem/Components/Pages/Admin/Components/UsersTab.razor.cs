using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin panel users management tab component.
/// </summary>
public partial class UsersTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<User>? Users { get; set; }

    [Parameter]
    public EventCallback OnUsersChanged { get; set; }

    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected string SearchQuery { get; set; } = string.Empty;

    protected static string GetInitials(string firstName, string lastName)
    {
        var f = string.IsNullOrWhiteSpace(firstName) ? string.Empty : firstName[0].ToString().ToUpperInvariant();
        var l = string.IsNullOrWhiteSpace(lastName) ? string.Empty : lastName[0].ToString().ToUpperInvariant();
        return $"{f}{l}";
    }

    protected static string GetAvatarClass(UserRole role) => role switch
    {
        UserRole.Admin => "avatar-admin",
        UserRole.Developer => "avatar-dev",
        _ => "avatar-user",
    };

    protected static string GetRoleSelectClass(UserRole role) => role switch
    {
        UserRole.Admin => "role-admin",
        UserRole.Developer => "role-dev",
        _ => "role-user",
    };

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
                u.Person.FirstName.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                u.Person.LastName.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase))
            .OrderBy(u => u.Person.LastName, StringComparer.OrdinalIgnoreCase)
            .ToList();

    protected bool CanEditUserRole(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        return user.Id != this.AuthService.CurrentUser?.Id;
    }

    protected async Task UpdateUserRole(int userId, UserRole newRole)
    {
        var user = this.Users?.FirstOrDefault(u => u.Id == userId);
        if (user is not null && !this.CanEditUserRole(user))
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotEditSelfRole"), ToastType.Error);
            await this.OnUsersChanged.InvokeAsync();
            return;
        }

        var success = await this.AdminService.UpdateUserRoleAsync(userId, newRole);
        if (success)
        {
            if (user is not null)
            {
                user.Role = newRole;
                await this.InvokeAsync(this.StateHasChanged);
            }

            await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
            await this.OnUsersChanged.InvokeAsync();
        }
    }

    protected async Task ToggleUserStatus(int userId)
    {
        var user = this.Users?.FirstOrDefault(u => u.Id == userId);
        if (user?.Role == UserRole.Admin)
        {
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeactivateAdmin"), ToastType.Error);
            return;
        }

        var success = await this.AdminService.ToggleUserActiveStatusAsync(userId);
        if (success && user is not null)
        {
            user.IsActive = !user.IsActive;
            await this.InvokeAsync(this.StateHasChanged);
            await this.ToastService.ShowToastAsync(this.L.Translate("admin.userUpdated"), ToastType.Success);
            await this.OnUsersChanged.InvokeAsync();
        }
    }

    protected async Task DeleteUser(User user)
    {
        ArgumentNullException.ThrowIfNull(user);

        try
        {
            if (user.Role == UserRole.Admin)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteAdmin"), ToastType.Error);
                return;
            }

            var success = await this.AdminService.DeleteUserAsync(user.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.userDeleted"), ToastType.Success);
                this.Users?.Remove(user);
                await this.InvokeAsync(this.StateHasChanged);
                await this.OnUsersChanged.InvokeAsync();
            }
            else
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("admin.cannotDeleteUserWithTickets"), ToastType.Error);
            }
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"Error deleting user: {ex.Message}", ToastType.Error);
        }
    }
}
