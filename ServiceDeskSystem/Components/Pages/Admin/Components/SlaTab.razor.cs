using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Sla;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

public partial class SlaTab : BaseComponent
{
    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<SlaPolicy>? Policies { get; set; }

    [Parameter]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1002:Do not expose generic lists", Justification = "Blazor parameter")]
    public IList<Product>? Products { get; set; }

    [Parameter]
    public EventCallback OnPoliciesChanged { get; set; }

    [Inject]
    protected ISlaPolicyService SlaPolicyService { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected SlaPolicy EditingPolicy { get; set; } = new ();

    protected int SelectedProductId { get; set; }

    protected bool ShowModal { get; set; }

    protected string ModalTitle { get; set; } = string.Empty;

    protected bool IsEditing { get; set; }

    protected bool IsSaving { get; set; }

    protected bool IsRecalculating { get; set; }

    protected string? ErrorMessage { get; set; }

    protected string SearchQuery { get; set; } = string.Empty;

    protected string SortColumn { get; set; } = "priority";

    protected bool SortAscending { get; set; }

    protected static string GetPriorityClass(TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => "is-critical",
        TicketPriority.High => "is-high",
        TicketPriority.Medium => "is-medium",
        _ => "is-low",
    };

    protected IList<SlaPolicy>? GetFilteredPolicies()
    {
        var query = this.Policies?
            .Where(p =>
                string.IsNullOrWhiteSpace(this.SearchQuery) ||
                p.Name.Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                p.Priority.ToString().Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.Product?.Name ?? string.Empty).Contains(this.SearchQuery, StringComparison.OrdinalIgnoreCase));

        if (query is null)
        {
            return null;
        }

        var ordered = this.SortColumn == "name"
            ? query.OrderBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
            : query.OrderBy(p => (int)p.Priority);

        if (!this.SortAscending)
        {
            ordered = this.SortColumn == "name"
                ? query.OrderByDescending(p => p.Name, StringComparer.OrdinalIgnoreCase)
                : query.OrderByDescending(p => (int)p.Priority);
        }

        return ordered.ThenBy(p => p.ProductId ?? 0).ToList();
    }

    protected void SetSort(string column)
    {
        if (this.SortColumn == column)
        {
            this.SortAscending = !this.SortAscending;
        }
        else
        {
            this.SortColumn = column;
            this.SortAscending = true;
        }
    }

    protected string GetSortArrow(string column)
    {
        if (this.SortColumn != column)
        {
            return "↕";
        }

        return this.SortAscending ? "↑" : "↓";
    }

    protected void ShowAddPolicy()
    {
        this.EditingPolicy = new SlaPolicy
        {
            Priority = TicketPriority.Medium,
            ResponseMinutes = 480,
            ResolutionMinutes = 2880,
            WarningThresholdPercent = SlaPolicy.DefaultWarningThresholdPercent,
            IsActive = true,
        };
        this.SelectedProductId = 0;
        this.OpenModal(this.L.Translate("sla.addPolicy"), isEdit: false);
    }

    protected void EditPolicy(SlaPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        this.EditingPolicy = new SlaPolicy
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            Priority = policy.Priority,
            ProductId = policy.ProductId,
            ResponseMinutes = policy.ResponseMinutes,
            ResolutionMinutes = policy.ResolutionMinutes,
            WarningThresholdPercent = policy.WarningThresholdPercent,
            EscalateOnBreach = policy.EscalateOnBreach,
            IsActive = policy.IsActive,
        };
        this.SelectedProductId = policy.ProductId ?? 0;
        this.OpenModal(this.L.Translate("sla.editPolicy"), isEdit: true);
    }

    protected async Task ToggleActiveAsync(SlaPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        try
        {
            await this.SlaPolicyService.SetActiveAsync(policy.Id, !policy.IsActive, this.AuthService.CurrentUser?.Id);
            await this.OnPoliciesChanged.InvokeAsync();
        }
        catch (InvalidOperationException ex)
        {
            await this.ToastService.ShowToastAsync(ex.Message, ToastType.Error);
        }
    }

    protected async Task DeletePolicyAsync(SlaPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        try
        {
            var success = await this.SlaPolicyService.DeletePolicyAsync(policy.Id, this.AuthService.CurrentUser?.Id);
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.deleted"), ToastType.Success);
                await this.OnPoliciesChanged.InvokeAsync();
            }
        }
        catch (InvalidOperationException ex)
        {
            await this.ToastService.ShowToastAsync(ex.Message, ToastType.Error);
        }
    }

    protected async Task RecalculateAsync()
    {
        this.IsRecalculating = true;

        try
        {
            var updated = await this.SlaPolicyService.ReapplyToOpenTicketsAsync(this.AuthService.CurrentUser?.Id);
            var message = string.Format(
                CultureInfo.CurrentCulture,
                this.L.Translate("sla.recalculated"),
                updated);
            await this.ToastService.ShowToastAsync(message, ToastType.Success);
            await this.OnPoliciesChanged.InvokeAsync();
        }
        catch (InvalidOperationException ex)
        {
            await this.ToastService.ShowToastAsync(ex.Message, ToastType.Error);
        }
        finally
        {
            this.IsRecalculating = false;
        }
    }

    protected void CloseModal()
    {
        this.ShowModal = false;
        this.ErrorMessage = null;
    }

    protected async Task SaveModalAsync()
    {
        this.IsSaving = true;
        this.ErrorMessage = null;

        try
        {
            this.EditingPolicy.ProductId = this.SelectedProductId == 0 ? null : this.SelectedProductId;

            if (this.IsEditing)
            {
                await this.SlaPolicyService.UpdatePolicyAsync(this.EditingPolicy, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.updated"), ToastType.Success);
            }
            else
            {
                await this.SlaPolicyService.CreatePolicyAsync(this.EditingPolicy, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.created"), ToastType.Success);
            }

            await this.OnPoliciesChanged.InvokeAsync();
            this.CloseModal();
        }
        catch (ArgumentException ex)
        {
            this.ErrorMessage = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            this.ErrorMessage = ex.Message;
        }
        finally
        {
            this.IsSaving = false;
        }
    }

    private void OpenModal(string title, bool isEdit)
    {
        this.ModalTitle = title;
        this.IsEditing = isEdit;
        this.ErrorMessage = null;
        this.ShowModal = true;
    }
}
