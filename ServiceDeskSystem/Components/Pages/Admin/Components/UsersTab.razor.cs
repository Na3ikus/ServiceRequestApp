using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin.Interfaces;
using ServiceDeskSystem.Application.Services.Auth.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Interfaces;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

public partial class UsersTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public List<User>? Users { get; set; }

    [Parameter]
    public EventCallback OnUsersChanged { get; set; }

    [Inject]
    private IAdminService AdminService { get; set; } = null!;

    [Inject]
    private IAuthService AuthService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    private static bool CanDeleteUser(User user) => user.Role != UserRole.Admin;

    private static bool CanToggleUserStatus(User user) => user.Role != UserRole.Admin;

    private bool CanEditUserRole(User user) => user.Id != this.AuthService.CurrentUser?.Id;

    private async Task UpdateUserRole(int userId, UserRole newRole)
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

    private async Task ToggleUserStatus(int userId)
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

    private async Task DeleteUser(User user)
    {
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
