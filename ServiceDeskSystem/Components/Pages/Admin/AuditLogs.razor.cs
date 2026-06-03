using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Auth;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Audit logs page for tracking user actions and system changes.
/// </summary>
public partial class AuditLogs : BaseComponent
{
    private List<AuditLog>? logs;
    private string searchTerm = string.Empty;
    private bool showClearConfirm;
    private CancellationTokenSource? cts;

    [Inject]
    protected IAuthService AuthService { get; set; } = null!;

    [Inject]
    protected IAuditService AuditService { get; set; } = null!;

    protected IEnumerable<AuditLog> FilteredLogs =>
        string.IsNullOrWhiteSpace(this.searchTerm)
            ? (this.logs ?? Enumerable.Empty<AuditLog>())
            : (this.logs ?? Enumerable.Empty<AuditLog>()).Where(l =>
                (l.Action?.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.User?.Login?.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.EntityName?.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.EntityId?.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (l.Changes?.Contains(this.searchTerm, StringComparison.OrdinalIgnoreCase) ?? false));

    protected override async Task OnInitializedAsync()
    {
        if (this.AuthService.CurrentUser?.Role == UserRole.Admin)
        {
            await this.LoadLogsAsync();
            this.StartAutoRefresh();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.cts?.Cancel();
            this.cts?.Dispose();
        }

        base.Dispose(disposing);
    }

    protected void RequestClearLogs()
    {
        this.showClearConfirm = true;
    }

    protected void CancelClearLogs()
    {
        this.showClearConfirm = false;
    }

    protected async Task ConfirmClearLogsAsync()
    {
        this.showClearConfirm = false;
        await this.AuditService.ClearAllLogsAsync();
        await this.LoadLogsAsync();
        this.StateHasChanged();
    }

    protected string GetActionClass(string action) => action.ToUpperInvariant() switch
    {
        "CREATE" => "bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400",
        "UPDATESTATUS" => "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
        "UPDATEDATES" => "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400",
        "ASSIGN" => "bg-indigo-100 text-indigo-700 dark:bg-indigo-900/30 dark:text-indigo-400",
        "DELETE" => "bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400",
        _ => "bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300"
    };

    private async Task LoadLogsAsync()
    {
        this.logs = await this.AuditService.GetLatestLogsAsync();
    }

    private void StartAutoRefresh()
    {
        this.cts = new CancellationTokenSource();
        _ = this.RunPeriodicRefreshAsync(this.cts.Token);
    }

    private async Task RunPeriodicRefreshAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(15));
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (await timer.WaitForNextTickAsync(token))
                {
                    await this.InvokeAsync(async () =>
                    {
                        await this.LoadLogsAsync();
                        this.StateHasChanged();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch
            {
                // Ignore refresh exceptions
            }
        }
    }
}

