using System.Globalization;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Sla;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// Admin SLA section: policies with the active switch in the row, a two-row inline edit and a delete prompt.
/// </summary>
public partial class SlaTab : BaseComponent
{
    private ElementReference firstField;

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

    protected RowState Row { get; } = new ();

    protected TableSort Sort { get; } = new ("priority", descending: true);

    protected bool IsRecalculating { get; set; }

    protected string SearchQuery { get; set; } = string.Empty;

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

        var ordered = this.Sort.Column == "name"
            ? this.Sort.Apply(query, p => p.Name, StringComparer.OrdinalIgnoreCase)
            : this.Sort.Apply(query, p => (int)p.Priority);

        return ordered.ThenBy(p => p.ProductId ?? 0).ToList();
    }

    protected void SortBy(string column) => this.Sort.Toggle(column);

    protected void ClearSearch() => this.SearchQuery = string.Empty;

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
        this.Row.Add();
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
        this.Row.Edit(policy.Id);
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

        this.Row.Begin();
        try
        {
            var success = await this.SlaPolicyService.DeletePolicyAsync(policy.Id, this.AuthService.CurrentUser?.Id);
            this.Row.Close();
            if (success)
            {
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.deleted"), ToastType.Success);
                await this.OnPoliciesChanged.InvokeAsync();
            }
        }
        catch (InvalidOperationException ex)
        {
            this.Row.Close();
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

    protected async Task SaveAsync()
    {
        var isNew = this.Row.IsAdding;
        this.Row.Begin();

        try
        {
            this.EditingPolicy.ProductId = this.SelectedProductId == 0 ? null : this.SelectedProductId;

            if (isNew)
            {
                await this.SlaPolicyService.CreatePolicyAsync(this.EditingPolicy, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.created"), ToastType.Success);
            }
            else
            {
                await this.SlaPolicyService.UpdatePolicyAsync(this.EditingPolicy, this.AuthService.CurrentUser?.Id);
                await this.ToastService.ShowToastAsync(this.L.Translate("sla.updated"), ToastType.Success);
            }

            this.Row.Close();
            await this.OnPoliciesChanged.InvokeAsync();
        }
        catch (ArgumentException ex)
        {
            this.Row.Fail(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            this.Row.Fail(ex.Message);
        }
    }
}
