using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Toasts;
using ServiceDeskSystem.Application.Services.Toasts.Models;
using ServiceDeskSystem.Components.UI.Base;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using PathSegment = ServiceDeskSystem.Components.UI.Primitives.PathSegment;

namespace ServiceDeskSystem.Components.Pages.Admin;

/// <summary>
/// Audit log stream: events grouped by day in the user's zone, details and diff in place, live updates.
/// </summary>
public partial class AuditLogs : BaseComponent
{
    private const int PageSize = 50;

    private static readonly IReadOnlyList<PathSegment> Segments = [new ("audit-logs")];

    // Level and period labels stay English, like the section names (Stage 3 decision 5).
    private static readonly IReadOnlyList<(string Value, string Text)> SeverityOptions =
    [
        ("ALL", "all"),
        ("Critical", "crit"),
        ("Warning", "warn"),
        ("Info", "info"),
    ];

    private static readonly IReadOnlyList<(string Value, string Text)> PeriodOptions =
    [
        ("TODAY", "today"),
        ("WEEK", "7d"),
        ("MONTH", "30d"),
        ("ALL", "all"),
    ];

    private List<AuditLog>? logs;
    private string searchTerm = string.Empty;
    private string selectedEntity = "ALL";
    private string selectedCategory = "ALL";
    private string selectedSeverity = "ALL";
    private string selectedTimeRange = "ALL";
    private string selectedUserIdString = "ALL";
    private int visibleCount = PageSize;
    private int? openLogId;
    private bool showClearConfirm;
    private bool isClearing;
    private bool isAutoRefreshPaused;
    private bool isRefreshing;
    private int pendingNewEventsCount;
    private CancellationTokenSource? cts;
    private HubConnection? hubConnection;

    /// <summary>
    /// State of the updates hub shown in the header.
    /// </summary>
    protected enum LiveState
    {
        /// <summary>Starting or reconnecting.</summary>
        Connecting,

        /// <summary>Connected; new events arrive live.</summary>
        Live,

        /// <summary>The first start failed or reconnecting gave up; only the 20 s refresh runs.</summary>
        Offline,
    }

    [Inject]
    protected IAuditService AuditService { get; set; } = null!;

    [Inject]
    protected IJSRuntime JS { get; set; } = null!;

    [Inject]
    protected IToastService ToastService { get; set; } = null!;

    protected bool IsAdmin => this.AuthService.CurrentUser?.Role == UserRole.Admin;

    protected LiveState Live { get; set; } = LiveState.Connecting;

    protected string SearchTerm
    {
        get => this.searchTerm;
        set => this.SetFilter(ref this.searchTerm, value);
    }

    protected string SelectedEntity
    {
        get => this.selectedEntity;
        set => this.SetFilter(ref this.selectedEntity, value);
    }

    protected string SelectedCategory
    {
        get => this.selectedCategory;
        set => this.SetFilter(ref this.selectedCategory, value);
    }

    protected string SelectedSeverity
    {
        get => this.selectedSeverity;
        set => this.SetFilter(ref this.selectedSeverity, value);
    }

    protected string SelectedTimeRange
    {
        get => this.selectedTimeRange;
        set => this.SetFilter(ref this.selectedTimeRange, value);
    }

    protected string SelectedUserIdString
    {
        get => this.selectedUserIdString;
        set => this.SetFilter(ref this.selectedUserIdString, value);
    }

    protected int TotalLogsCount => this.logs?.Count ?? 0;

    protected int TodayLogsCount
    {
        get
        {
            var today = this.UserZone.Today;
            return this.logs?.Count(l => this.LocalDate(l) == today) ?? 0;
        }
    }

    protected int CriticalCount => this.logs?.Count(l => GetLogSeverity(l) == "Critical") ?? 0;

    protected int WarningCount => this.logs?.Count(l => GetLogSeverity(l) == "Warning") ?? 0;

    protected int UniqueUsersCount => this.logs?.Select(l => l.UserId ?? 0).Where(id => id > 0).Distinct().Count() ?? 0;

    /// <summary>
    /// Gets a value indicating whether the list is held still while the reader works with it (paused, an event open,
    /// filters set or more than one page shown): new events then only count towards the "new events" bar.
    /// </summary>
    protected bool IsHeld =>
        this.isAutoRefreshPaused || this.openLogId is not null || this.HasActiveFilters || this.visibleCount > PageSize;

    protected bool HasActiveFilters =>
        !string.IsNullOrWhiteSpace(this.searchTerm) ||
        this.selectedEntity != "ALL" ||
        this.selectedCategory != "ALL" ||
        this.selectedSeverity != "ALL" ||
        this.selectedTimeRange != "ALL" ||
        this.selectedUserIdString != "ALL";

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

            if (!string.IsNullOrEmpty(this.selectedEntity) && this.selectedEntity != "ALL")
            {
                query = query.Where(l => string.Equals(l.EntityName, this.selectedEntity, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(this.selectedCategory) && this.selectedCategory != "ALL")
            {
                query = query.Where(l => GetActionCategory(l.Action) == this.selectedCategory);
            }

            if (!string.IsNullOrEmpty(this.selectedSeverity) && this.selectedSeverity != "ALL")
            {
                query = query.Where(l => string.Equals(GetLogSeverity(l), this.selectedSeverity, StringComparison.OrdinalIgnoreCase));
            }

            if (int.TryParse(this.selectedUserIdString, out var userId) && userId > 0)
            {
                query = query.Where(l => l.UserId == userId);
            }

            // Periods are whole days in the user's zone: today, today and the 6 days before, or 29 days before.
            var today = this.UserZone.Today;
            DateOnly? since = this.selectedTimeRange switch
            {
                "TODAY" => today,
                "WEEK" => today.AddDays(-6),
                "MONTH" => today.AddDays(-29),
                _ => null,
            };

            if (since is DateOnly from)
            {
                query = query.Where(l => this.LocalDate(l) >= from);
            }

            return query;
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

    protected static AuditChangePayload? GetParsedPayload(AuditLog? log)
    {
        return AuditChangePayload.TryParse(log?.Changes);
    }

    protected static string GetLogDisplaySummary(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);

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

    /// <summary>
    /// Level column: <c>CRIT</c> in danger, <c>WARN</c> in fg (there is no warning token), <c>info</c> muted.
    /// </summary>
    protected static (string Text, string Class) GetLevel(AuditLog log) => GetLogSeverity(log) switch
    {
        "Critical" => ("CRIT", "text-danger"),
        "Warning" => ("WARN", "text-fg"),
        _ => ("info", "text-muted"),
    };

    protected static string GetUserLabel(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return $"@{log.User?.Login ?? "system"}";
    }

    protected static string GetEntityLabel(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return string.IsNullOrWhiteSpace(log.EntityId) ? log.EntityName : $"{log.EntityName}#{log.EntityId}";
    }

    protected static bool CanNavigateToEntity(AuditLog? log)
    {
        return log != null &&
               string.Equals(log.EntityName, "Ticket", StringComparison.OrdinalIgnoreCase) &&
               int.TryParse(log.EntityId, out _);
    }

    /// <summary>
    /// Day heading: <c>today · 11 Oct</c>, <c>yesterday · 10 Oct</c>, then <c>9 Oct 2026</c>.
    /// </summary>
    protected string GetDayHeading(DateOnly day)
    {
        var today = this.UserZone.Today;
        if (day == today)
        {
            return $"{this.Heading("audit.today")} · {day.ToString("d MMM", this.UiCulture)}";
        }

        if (day == today.AddDays(-1))
        {
            return $"{this.Heading("audit.yesterday")} · {day.ToString("d MMM", this.UiCulture)}";
        }

        return day.ToString("d MMM yyyy", this.UiCulture);
    }

    protected DateOnly LocalDate(AuditLog log)
    {
        ArgumentNullException.ThrowIfNull(log);
        return DateOnly.FromDateTime(this.ToLocal(log.Timestamp));
    }

    protected override async Task OnInitializedAsync()
    {
        if (this.IsAdmin)
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

    protected void ToggleLog(int id)
    {
        this.openLogId = this.openLogId == id ? null : id;
    }

    protected void ShowMore()
    {
        this.visibleCount += PageSize;
    }

    protected void RequestClearLogs()
    {
        this.showClearConfirm = true;
    }

    protected void CancelClearLogs()
    {
        // A running clear is not cancelled: the prompt stays until it finishes.
        if (!this.isClearing)
        {
            this.showClearConfirm = false;
        }
    }

    protected async Task ConfirmClearLogsAsync()
    {
        if (this.isClearing)
        {
            return;
        }

        this.isClearing = true;
        try
        {
            await this.AuditService.ClearAllLogsAsync();
        }
        catch (Exception ex)
        {
            await this.ToastService.ShowToastAsync($"{this.L.Translate("audit.clearFailed")}: {ex.Message}", ToastType.Error);
            return;
        }
        finally
        {
            this.isClearing = false;
            this.showClearConfirm = false;
        }

        this.openLogId = null;
        this.ResetFilters();
        await this.LoadLogsAsync();
    }

    protected void ResetFilters()
    {
        this.searchTerm = string.Empty;
        this.selectedEntity = "ALL";
        this.selectedCategory = "ALL";
        this.selectedSeverity = "ALL";
        this.selectedTimeRange = "ALL";
        this.selectedUserIdString = "ALL";
        this.visibleCount = PageSize;
    }

    protected void ToggleAutoRefresh()
    {
        this.isAutoRefreshPaused = !this.isAutoRefreshPaused;
    }

    protected async Task RefreshNowAsync()
    {
        this.isRefreshing = true;
        try
        {
            await this.LoadLogsAsync();
        }
        finally
        {
            this.isRefreshing = false;
        }
    }

    protected async Task ApplyPendingEventsAsync()
    {
        this.visibleCount = PageSize;
        await this.LoadLogsAsync();
    }

    protected async Task ExportToCsvAsync()
    {
        var logsToExport = this.FilteredLogs.ToList();
        if (logsToExport.Count == 0)
        {
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Id,TimestampUtc,Severity,User,Action,EntityName,EntityId,IpAddress,Changes");

        foreach (var log in logsToExport)
        {
            var id = log.Id;
            var tsUtc = EscapeCsv(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
            var severity = EscapeCsv(GetLogSeverity(log));
            var user = EscapeCsv(log.User?.Login ?? "System");
            var action = EscapeCsv(log.Action);
            var entityName = EscapeCsv(log.EntityName);
            var entityId = EscapeCsv(log.EntityId);
            var payload = GetParsedPayload(log);
            var ip = EscapeCsv(payload?.IpAddress ?? string.Empty);
            var changes = EscapeCsv(payload?.Summary ?? log.Changes ?? string.Empty);

            sb.AppendLine(CultureInfo.InvariantCulture, $"{id},{tsUtc},{severity},{user},{action},{entityName},{entityId},{ip},{changes}");
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = preamble.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        using var stream = new MemoryStream(bytes);
        using var streamRef = new DotNetStreamReference(stream);
        var fileName = $"audit-logs-{this.ToLocal(DateTime.UtcNow):yyyyMMdd-HHmmss}.csv";
        await this.JS.InvokeVoidAsync("downloadFileFromStream", fileName, streamRef);
    }

    /// <summary>
    /// Connects to the updates hub; <see cref="OnAuditLogsChangedAsync"/> runs for every new event.
    /// </summary>
    protected virtual async Task StartHubAsync()
    {
        try
        {
            this.hubConnection = new HubConnectionBuilder()
                .WithUrl(this.Navigation.ToAbsoluteUri("/hubs/updates"))
                .WithAutomaticReconnect()
                .Build();

            this.hubConnection.On("AuditLogsChanged", this.OnAuditLogsChangedAsync);
            this.hubConnection.Reconnecting += _ => this.SetLiveAsync(LiveState.Connecting);
            this.hubConnection.Reconnected += _ => this.SetLiveAsync(LiveState.Live);
            this.hubConnection.Closed += _ => this.SetLiveAsync(LiveState.Offline);

            await this.hubConnection.StartAsync();
            this.Live = LiveState.Live;
        }
        catch
        {
            // Automatic reconnect does not retry a failed first start; the 20 s refresh still runs.
            this.Live = LiveState.Offline;
        }
    }

    /// <summary>
    /// Reloads the list, or counts the event for the "new events" bar while the list is held (<see cref="IsHeld"/>).
    /// </summary>
    protected Task OnAuditLogsChangedAsync() => this.InvokeAsync(async () =>
    {
        if (this.IsHeld)
        {
            this.pendingNewEventsCount++;
        }
        else
        {
            await this.LoadLogsAsync();
        }

        this.StateHasChanged();
    });

    /// <summary>
    /// Fallback refresh every 20 s, skipped while the list is held (<see cref="IsHeld"/>).
    /// </summary>
    protected Task OnAutoRefreshTickAsync() => this.InvokeAsync(async () =>
    {
        if (this.IsHeld)
        {
            return;
        }

        await this.LoadLogsAsync();
        this.StateHasChanged();
    });

    private static string EscapeCsv(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "\"\"";
        }

        var escaped = text.Replace("\"", "\"\"", StringComparison.OrdinalIgnoreCase);
        return $"\"{escaped}\"";
    }

    private void SetFilter(ref string field, string value)
    {
        if (field != value)
        {
            field = value;
            this.visibleCount = PageSize;
            this.openLogId = null;
        }
    }

    private Task SetLiveAsync(LiveState state)
    {
        this.Live = state;
        return this.InvokeAsync(this.StateHasChanged);
    }

    // Everything is listed after a load, so held events are no longer pending. An open event that is gone
    // (cleared, out of the 500, filtered out or past the shown page) is closed, so it does not hold the list.
    private async Task LoadLogsAsync()
    {
        this.logs = await this.AuditService.GetLatestLogsAsync(500);
        this.pendingNewEventsCount = 0;
        if (this.openLogId is int id && !this.FilteredLogs.Take(this.visibleCount).Any(l => l.Id == id))
        {
            this.openLogId = null;
        }
    }

    private async Task StopHubAsync()
    {
        if (this.hubConnection is not null)
        {
            try
            {
                await this.hubConnection.StopAsync();
                await this.hubConnection.DisposeAsync();
            }
            catch
            {
                // Ignore hub disconnect errors
            }
            finally
            {
                this.hubConnection = null;
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
                    await this.OnAutoRefreshTickAsync();
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
