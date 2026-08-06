#pragma warning disable CA1724
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Admin;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Application.Services.Tags;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Admin panel page for managing products, tech stacks, tags, and users.
/// </summary>
public partial class Admin : BaseComponent
{
#pragma warning restore CA1724
    [Inject]
    protected IAdminService AdminService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    [Inject]
    protected IEmailSender EmailSender { get; set; } = null!;

    [Inject]
    protected ITagService TagService { get; set; } = null!;

    protected List<Product>? Products { get; set; }

    protected List<TechStack>? TechStacks { get; set; }

    protected List<User>? Users { get; set; }

    protected List<Tag>? Tags { get; set; }

    protected string ActiveTab { get; set; } = "products";

    protected bool IsMobileNavOpen { get; set; }

    protected bool IsCheckingSmtp { get; set; }

    protected bool? SmtpCheckSuccess { get; set; }

    protected string? SmtpCheckMessage { get; set; }

    protected bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

    protected override async Task OnInitializedAsync()
    {
        if (this.IsAdmin)
        {
            await this.LoadDataAsync().ConfigureAwait(false);
            await this.CheckSmtpStatusAsync(showToast: false).ConfigureAwait(false);
        }
    }

    protected async Task LoadDataAsync()
    {
        if (!this.IsAdmin)
        {
            return;
        }

        this.TechStacks = await this.AdminService.GetAllTechStacksAsync().ConfigureAwait(false);
        this.Products = await this.AdminService.GetAllProductsAsync().ConfigureAwait(false);
        this.Users = await this.AdminService.GetAllUsersAsync().ConfigureAwait(false);
        this.Tags = (await this.TagService.GetAllTagsAsync().ConfigureAwait(false)).ToList();
    }

    protected void SetActiveTab(string tab)
    {
        this.ActiveTab = tab;
        this.IsMobileNavOpen = false;
    }

    protected void ToggleMobileNav() => this.IsMobileNavOpen = !this.IsMobileNavOpen;

    protected void CloseMobileNav() => this.IsMobileNavOpen = false;

    protected string GetActiveTabLabel() => this.ActiveTab switch
    {
        "products" => this.L.Translate("admin.products"),
        "techstacks" => this.L.Translate("admin.techStacks"),
        "users" => this.L.Translate("admin.users"),
        "tags" => this.L.Translate("admin.tags"),
        "smtp" => this.L.Translate("admin.smtp"),
        _ => string.Empty,
    };

    protected async Task CheckSmtpAsync()
    {
        await this.CheckSmtpStatusAsync(showToast: true).ConfigureAwait(false);
    }

    protected async Task CheckSmtpStatusAsync(bool showToast)
    {
        if (this.IsCheckingSmtp)
        {
            return;
        }

        this.IsCheckingSmtp = true;
        this.SmtpCheckMessage = null;

        try
        {
            var (isSuccess, message) = await this.EmailSender.CheckConnectionAsync().ConfigureAwait(false);
            this.SmtpCheckSuccess = isSuccess;
            this.SmtpCheckMessage = message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync(
                    isSuccess ? "SMTP connection is healthy." : $"SMTP check failed: {message}",
                    isSuccess ? ToastType.Success : ToastType.Error).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            this.SmtpCheckSuccess = false;
            this.SmtpCheckMessage = ex.Message;

            if (showToast)
            {
                await this.ToastService.ShowToastAsync($"SMTP check error: {ex.Message}", ToastType.Error).ConfigureAwait(false);
            }
        }
        finally
        {
            this.IsCheckingSmtp = false;
        }
    }
}
