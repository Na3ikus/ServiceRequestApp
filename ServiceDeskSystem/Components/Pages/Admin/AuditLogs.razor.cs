using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Audit logs page for tracking user actions and system changes with advanced analytics, filters, inspector, visual diff, severity levels and SignalR live feed.
/// </summary>
public partial class AuditLogs : BaseComponent
{
    private List<AuditLog>? logs;
    private string searchTerm = string.Empty;
    private string selectedEntity = "ALL";
    private string selectedCategory = "ALL";
    private string selectedSeverity = "ALL";
    private string selectedTimeRange = "ALL";
    private string selectedUserIdString = "ALL";
    private bool showClearConfirm;
    private bool isAutoRefreshPaused;
    private bool isRefreshing;
    private CancellationTokenSource? cts;

    // SignalR Live connection
    private HubConnection? hubConnection;
    private bool isRealtimeConnected;
    private int pendingNewEventsCount;

    // Pagination
    private int currentPage = 1;
    private int pageSize = 25;

    // Details Modal
    private AuditLog? selectedLog;
    private bool showDetailsModal;

    [Inject]
    protected IAuditService AuditService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = null!;

    // Metrics
    protected int TotalLogsCount => this.logs?.Count ?? 0;

    protected int TodayLogsCount => this.logs?.Count(l => l.Timestamp >= DateTime.UtcNow.Date) ?? 0;

    protected int CreatesCount => this.logs?.Count(l => GetActionCategory(l.Action) == "CREATE") ?? 0;

    protected int UpdatesCount => this.logs?.Count(l => GetActionCategory(l.Action) == "UPDATE") ?? 0;

    protected int DeletesCount => this.logs?.Count(l => GetActionCategory(l.Action) == "DELETE") ?? 0;

    protected int CriticalCount => this.logs?.Count(l => GetLogSeverity(l) == "Critical") ?? 0;

    protected int WarningCount => this.logs?.Count(l => GetLogSeverity(l) == "Warning") ?? 0;

    protected int UniqueUsersCount => this.logs?.Select(l => l.UserId ?? 0).Where(id => id > 0).Distinct().Count() ?? 0;

    protected bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(this.searchTerm) ||
        this.selectedEntity != "ALL" ||
        this.selectedCategory != "ALL" ||
        this.selectedSeverity != "ALL" ||
        this.selectedTimeRange != "ALL" ||
        this.selectedUserIdString != "ALL";

    // Available filters
    protected IEnumerable<string> AvailableEntities =>
        this.logs?.Select(l => l.EntityName).Where(e => !string.IsNullOrWhiteSpace(e)).Distinct().OrderBy(e => e)
        ?? Enumerable.Empty<string>();

    protected IEnumerable<(int Id, string Login)> AvailableUsers =>
        this.logs?.Where(l => l.User != null && l.UserId.HasValue)
            .Select(l => (l.UserId!.Value, l.User!.Login))
            .DistinctBy(u => u.Value)
            .OrderBy(u => u.Login)
        ?? Enumerable.Empty<(int, string)>();

    protected IEnumerable<AuditLog> FilteredLogs
    {
        get
        {
            if (this.logs is null)
            {
                return Enumerable.Empty<AuditLog>();
            }

            var query = this.logs.AsEnumerable();

            // Text search
            if (!string.IsNullOrWhiteSpace(this.searchTerm))
            {
                var term = this.searchTerm.Trim();
                query = query.Where(l =>
                    (l.Action?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.User?.Login?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.EntityName?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.EntityId?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (l.Changes?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
            }

            // Entity filter
            if (!string.IsNullOrEmpty(this.selectedEntity) && this.selectedEntity != "ALL")
            {
                query = query.Where(l => string.Equals(l.EntityName, this.selectedEntity, StringComparison.OrdinalIgnoreCase));
            }

            // Action Category filter
            if (!string.IsNullOrEmpty(this.selectedCategory) && this.selectedCategory != "ALL")
            {
                query = query.Where(l => GetActionCategory(l.Action) == this.selectedCategory);
            }

            // Severity filter
            if (!string.IsNullOrEmpty(this.selectedSeverity) && this.selectedSeverity != "ALL")
            {
                query = query.Where(l => string.Equals(GetLogSeverity(l), this.selectedSeverity, StringComparison.OrdinalIgnoreCase));
            }

            // User filter
            if (int.TryParse(this.selectedUserIdString, out var userId) && userId > 0)
            {
                query = query.Where(l => l.UserId == userId);
            }

            // Time range filter
            if (this.selectedTimeRange != "ALL")
            {
                var now = DateTime.UtcNow;
                query = this.selectedTimeRange switch
                {
                    "TODAY" => query.Where(l => l.Timestamp >= now.Date),
                    "WEEK" => query.Where(l => l.Timestamp >= now.AddDays(-7)),
                    "MONTH" => query.Where(l => l.Timestamp >= now.AddDays(-30)),
                    _ => query,
                };
            }

            return query;
        }
    }

    protected int FilteredCount => this.FilteredLogs.Count();

    protected int TotalPages => Math.Max(1, (int)Math.Ceiling(this.FilteredCount / (double)this.pageSize));

    protected IEnumerable<AuditLog> PagedLogs
    {
        get
        {
            var validPage = Math.Clamp(this.currentPage, 1, this.TotalPages);
            return this.FilteredLogs.Skip((validPage - 1) * this.pageSize).Take(this.pageSize);
        }
    }

    public static string GetLogSeverity(AuditLog? log)
    {
        if (log is null)
        {
            return "Info";
        }

        var payload = AuditChangePayload.TryParse(log.Changes);
        if (!string.IsNullOrWhiteSpace(payload?.Severity))
        {
            return payload.Severity;
        }

        var act = log.Action?.ToUpperInvariant() ?? string.Empty;
        if (act == "BRUTE_FORCE_BLOCKED" ||
            act.StartsWith("DELETE", StringComparison.OrdinalIgnoreCase) ||
            act == "CLEAR_LOGS")
        {
            return "Critical";
        }

        if (act == "LOGIN_FAILED" ||
            act == "UPDATE_USER_ROLE" ||
            act == "DEACTIVATE_USER")
        {
            return "Warning";
        }

        return "Info";
    }

    public static string GetSeverityBadgeClass(string severity)
    {
        return severity switch
        {
            "Critical" => "bg-rose-100 text-rose-800 dark:bg-rose-950/60 dark:text-rose-300 border border-rose-300 dark:border-rose-800/80",
            "Warning" => "bg-amber-100 text-amber-800 dark:bg-amber-950/60 dark:text-amber-300 border border-amber-300 dark:border-amber-800/80",
            _ => "bg-blue-50 text-blue-700 dark:bg-blue-950/50 dark:text-blue-300 border border-blue-200 dark:border-blue-800/60",
        };
    }

    protected static AuditChangePayload? GetParsedPayload(AuditLog? log)
    {
        return AuditChangePayload.TryParse(log?.Changes);
    }

    protected static string GetLogDisplaySummary(AuditLog log)
    {
        var payload = GetParsedPayload(log);
        if (!string.IsNullOrWhiteSpace(payload?.Summary))
        {
            return payload.Summary;
        }

        return log.Changes ?? string.Empty;
    }

    protected static string GetActionCategory(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return "OTHER";
        }

        var act = action.ToUpperInvariant();

        if (act == "CREATE" || act.StartsWith("CREATE_", StringComparison.OrdinalIgnoreCase) || act == "REGISTER")
        {
            return "CREATE";
        }

        if (act == "UPDATE" || act.StartsWith("UPDATE", StringComparison.OrdinalIgnoreCase))
        {
            return "UPDATE";
        }

        if (act == "DELETE" || act.StartsWith("DELETE_", StringComparison.OrdinalIgnoreCase))
        {
            return "DELETE";
        }

        if (act == "LOGIN" || act == "LOGOUT" || act == "LOGIN_FAILED" || act == "BRUTE_FORCE_BLOCKED")
        {
            return "AUTH";
        }

        if (act.Contains("ASSIGN", StringComparison.OrdinalIgnoreCase) ||
            act.Contains("TAG", StringComparison.OrdinalIgnoreCase) ||
            act.Contains("ATTACHMENT", StringComparison.OrdinalIgnoreCase) ||
            act.Contains("WORK_LOG", StringComparison.OrdinalIgnoreCase) ||
            act.Contains("COMMENT", StringComparison.OrdinalIgnoreCase))
        {
            return "ASSOCIATION";
        }

        return "OTHER";
    }

    protected static string GetActionClass(string? action)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return "bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-600";
        }

        var act = action.ToUpperInvariant();
        if (act == "BRUTE_FORCE_BLOCKED")
        {
            return "bg-rose-600 text-white font-black border border-rose-700 animate-pulse";
        }

        if (act == "LOGIN_FAILED")
        {
            return "bg-amber-100 text-amber-900 dark:bg-amber-900/50 dark:text-amber-200 border border-amber-300 dark:border-amber-800";
        }

        var category = GetActionCategory(action);
        return category switch
        {
            "CREATE" => "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/40 dark:text-emerald-300 border border-emerald-200 dark:border-emerald-800/60",
            "UPDATE" => "bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300 border border-blue-200 dark:border-blue-800/60",
            "DELETE" => "bg-rose-100 text-rose-800 dark:bg-rose-900/40 dark:text-rose-300 border border-rose-200 dark:border-rose-800/60",
            "AUTH" => "bg-purple-100 text-purple-800 dark:bg-purple-900/40 dark:text-purple-300 border border-purple-200 dark:border-purple-800/60",
            "ASSOCIATION" => "bg-indigo-100 text-indigo-800 dark:bg-indigo-900/40 dark:text-indigo-300 border border-indigo-200 dark:border-indigo-800/60",
            _ => "bg-gray-100 text-gray-700 dark:bg-gray-700 dark:text-gray-300 border border-gray-200 dark:border-gray-600",
        };
    }

    protected override async Task OnInitializedAsync()
    {
        if (this.AuthService.CurrentUser?.Role == UserRole.Admin)
        {
            await this.LoadLogsAsync();
            await this.StartHubAsync();
            this.StartAutoRefresh();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.cts?.Cancel();
            this.cts?.Dispose();
            _ = this.StopHubAsync();
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
        this.ResetFilters();
        this.StateHasChanged();
    }

    protected void ResetFilters()
    {
        this.searchTerm = string.Empty;
        this.selectedEntity = "ALL";
        this.selectedCategory = "ALL";
        this.selectedSeverity = "ALL";
        this.selectedTimeRange = "ALL";
        this.selectedUserIdString = "ALL";
        this.currentPage = 1;
    }

    protected void SetPage(int page)
    {
        if (page >= 1 && page <= this.TotalPages)
        {
            this.currentPage = page;
        }
    }

    protected void OnPageSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newSize) && newSize > 0)
        {
            this.pageSize = newSize;
            this.currentPage = 1;
        }
    }

    protected void ToggleAutoRefresh()
    {
        this.isAutoRefreshPaused = !this.isAutoRefreshPaused;
    }

    protected async Task RefreshNowAsync()
    {
        this.isRefreshing = true;
        this.pendingNewEventsCount = 0;
        try
        {
            await this.LoadLogsAsync();
        }
        finally
        {
            this.isRefreshing = false;
        }

        this.StateHasChanged();
    }

    protected async Task ApplyPendingEventsAsync()
    {
        this.pendingNewEventsCount = 0;
        this.currentPage = 1;
        await this.LoadLogsAsync();
        this.StateHasChanged();
    }

    protected void OpenInspector(AuditLog log)
    {
        this.selectedLog = log;
        this.showDetailsModal = true;
    }

    protected void CloseInspector()
    {
        this.showDetailsModal = false;
        this.selectedLog = null;
    }

    protected bool CanNavigateToEntity(AuditLog? log)
    {
        return log != null &&
               string.Equals(log.EntityName, "Ticket", StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(log.EntityId, out _);
    }

    protected void NavigateToEntity(AuditLog log)
    {
        if (this.CanNavigateToEntity(log))
        {
            this.NavigationManager.NavigateTo($"/tickets/{log.EntityId}");
        }
    }

    protected async Task ExportToCsvAsync()
    {
        var logsToExport = this.FilteredLogs.ToList();
        if (logsToExport.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id,TimestampUtc,TimestampLocal,Severity,User,Action,EntityName,EntityId,IpAddress,Changes");

        foreach (var log in logsToExport)
        {
            var id = log.Id;
            var tsUtc = EscapeCsv(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            var tsLocal = EscapeCsv(log.Timestamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
            var severity = EscapeCsv(GetLogSeverity(log));
            var user = EscapeCsv(log.User?.Login ?? "System");
            var action = EscapeCsv(log.Action);
            var entityName = EscapeCsv(log.EntityName);
            var entityId = EscapeCsv(log.EntityId);
            var payload = GetParsedPayload(log);
            var ip = EscapeCsv(payload?.IpAddress ?? string.Empty);
            var changes = EscapeCsv(payload?.Summary ?? log.Changes ?? string.Empty);

            sb.AppendLine(System.Globalization.CultureInfo.InvariantCulture, $"{id},{tsUtc},{tsLocal},{severity},{user},{action},{entityName},{entityId},{ip},{changes}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = preamble.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        var fileName = $"audit-logs-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
        await this.JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "\"\"";
        }

        var escaped = text.Replace("\"", "\"\"", StringComparison.OrdinalIgnoreCase);
        return $"\"{escaped}\"";
    }

    private async Task LoadLogsAsync()
    {
        this.logs = await this.AuditService.GetLatestLogsAsync(500);
    }

    private async Task StartHubAsync()
    {
        try
        {
            this.hubConnection = new HubConnectionBuilder()
                .WithUrl(this.NavigationManager.ToAbsoluteUri("/hubs/updates"))
                .WithAutomaticReconnect()
                .Build();

            this.hubConnection.On("AuditLogsChanged", async () =>
            {
                if (this.isAutoRefreshPaused || this.showDetailsModal || this.HasActiveFilters || this.currentPage > 1)
                {
                    this.pendingNewEventsCount++;
                    await this.InvokeAsync(this.StateHasChanged);
                }
                else
                {
                    await this.InvokeAsync(async () =>
                    {
                        await this.LoadLogsAsync();
                        this.StateHasChanged();
                    });
                }
            });

            this.hubConnection.Reconnected += _ =>
            {
                this.isRealtimeConnected = true;
                return this.InvokeAsync(this.StateHasChanged);
            };

            this.hubConnection.Closed += _ =>
            {
                this.isRealtimeConnected = false;
                return this.InvokeAsync(this.StateHasChanged);
            };

            await this.hubConnection.StartAsync().ConfigureAwait(false);
            this.isRealtimeConnected = true;
        }
        catch
        {
            this.isRealtimeConnected = false;
        }
    }

    private async Task StopHubAsync()
    {
        if (this.hubConnection is not null)
        {
            try
            {
                await this.hubConnection.StopAsync().ConfigureAwait(false);
                await this.hubConnection.DisposeAsync().ConfigureAwait(false);
            }
            catch
            {
                // Ignore hub disconnect errors
            }
            finally
            {
                this.hubConnection = null;
                this.isRealtimeConnected = false;
            }
        }
    }

    private void StartAutoRefresh()
    {
        this.cts = new CancellationTokenSource();
        _ = this.RunPeriodicRefreshAsync(this.cts.Token);
    }

    private async Task RunPeriodicRefreshAsync(CancellationToken token)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(20));
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (await timer.WaitForNextTickAsync(token))
                {
                    if (this.isAutoRefreshPaused || this.showDetailsModal)
                    {
                        continue;
                    }

                    // Passive fallback refresh
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
