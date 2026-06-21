#pragma warning disable CA1724
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Admin panel page for managing products, tech stacks, and users.
/// </summary>
public partial class Admin : BaseComponent
{
    #pragma warning restore CA1724
    [Inject]
    private IAdminService AdminService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    [Inject]
    private IEmailSender EmailSender { get; set; } = null!;

    private List<Product>? products { get; set; }

    private List<TechStack>? techStacks { get; set; }

    private List<User>? users { get; set; }

    private string activeTab { get; set; } = "products";

    private bool isMobileNavOpen { get; set; }

    private bool isCheckingSmtp { get; set; }

    private bool? smtpCheckSuccess { get; set; }

    private string? smtpCheckMessage { get; set; }

    private bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

    protected override async Task OnInitializedAsync()
    {
        if (this.IsAdmin)
        {
            await this.LoadDataAsync();
            await this.CheckSmtpStatusAsync(showToast: false);
        }
    }

    private async Task LoadDataAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.techStacks = await this.AdminService.GetAllTechStacksAsync();
        this.products = await this.AdminService.GetAllProductsAsync();
        this.users = await this.AdminService.GetAllUsersAsync();
    }

    private void SetActiveTab(string tab)
    {
        this.activeTab = tab;
        this.isMobileNavOpen = false;
    }

    private void ToggleMobileNav() => this.isMobileNavOpen = !this.isMobileNavOpen;

    private void CloseMobileNav() => this.isMobileNavOpen = false;

    private string GetActiveTabLabel() => this.activeTab switch
    {
        "products" => this.L.Translate("admin.products"),
        "techstacks" => this.L.Translate("admin.techStacks"),
        "users" => this.L.Translate("admin.users"),
        "smtp" => this.L.Translate("admin.smtp"),
        _ => string.Empty,
    };

    private async Task CheckSmtpAsync()
    {
        await this.CheckSmtpStatusAsync(showToast: true);
    }

    private async Task CheckSmtpStatusAsync(bool showToast)
    {
        if (this.isCheckingSmtp)
        {
            return;
        }

        this.isCheckingSmtp = true;
        this.smtpCheckMessage = null;

        try
        {
            var (isSuccess, message) = await this.EmailSender.CheckConnectionAsync().ConfigureAwait(false);
            this.smtpCheckSuccess = isSuccess;
            this.smtpCheckMessage = message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync(
                    isSuccess ? "SMTP connection is healthy." : $"SMTP check failed: {message}",
                    isSuccess ? ToastType.Success : ToastType.Error).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.smtpCheckSuccess = false;
            this.smtpCheckMessage = ex.Message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync($"SMTP check error: {ex.Message}", ToastType.Error).ConfigureAwait(false);
            }
        }
        finally
        {
            this.isCheckingSmtp = false;
        }
    }
}
