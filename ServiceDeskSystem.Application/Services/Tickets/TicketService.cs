
using ServiceDeskSystem.Application.Common.Models;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Application.Services.Notifications;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Application.Services.Tickets.Models;
using ServiceDeskSystem.Application.Services.Audit;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceDeskSystem.Application.Services.Tickets;

public sealed class TicketService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    INotificationService notificationService,
    IRealtimeNotifier realtimeNotifier,
    ServiceDeskSystem.Application.Common.IDomainEventDispatcher domainEventDispatcher,
    IMemoryCache memoryCache,
    IAuditService? auditService = null)
    : ITicketService, ITicketAssignmentService, ITicketStatisticsService
{
    private void ClearDashboardCache()
    {
        memoryCache.Remove("TicketCountByStatus");
        memoryCache.Remove("TicketCountByPriority");

        // AdminService caches products with their tickets (the admin "tickets" column).
        memoryCache.Remove("AllProducts");
    }


    public async Task<List<Ticket>> GetAllTicketsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false);
        return tickets.ToList();
    }


    public async Task<PagedResult<Ticket>> GetPagedTicketsAsync(int page, int pageSize)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var (items, totalCount) = await repo.Tickets.GetPagedWithIncludesAsync(page, pageSize).ConfigureAwait(false);
        return new PagedResult<Ticket>(items.ToList(), totalCount, page, pageSize);
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetByIdWithIncludesAsync(id).ConfigureAwait(false);
    }

    public async Task<List<TicketStatusHistory>> GetStatusHistoryAsync(int ticketId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.TicketStatusHistory.GetByTicketAsync(ticketId).ConfigureAwait(false);
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        await using var repo = repositoryFacadeFactory.Create();
        ticket.CreatedAt = DateTime.UtcNow;
        ticket.Status = TicketStatus.Open;

        await ApplySlaPolicyAsync(repo, ticket).ConfigureAwait(false);

        if (ticket.Type != TicketType.Project && !ticket.ProductId.HasValue)
        {
            throw new ArgumentException("Product is required for non-project tickets.", nameof(ticket));
        }

        if (!ticket.DomainEvents.Any(e => e is ServiceDeskSystem.Domain.Events.TicketCreatedEvent))
        {
            ticket.AddDomainEvent(new ServiceDeskSystem.Domain.Events.TicketCreatedEvent(0, ticket.AuthorId, ticket.Title));
        }

        await repo.Tickets.CreateAsync(ticket).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var events = ticket.DomainEvents.ToList();
        for (int i = 0; i < events.Count; i++)
        {
            if (events[i] is ServiceDeskSystem.Domain.Events.TicketCreatedEvent createdEvent && createdEvent.TicketId == 0)
            {
                events[i] = createdEvent with { TicketId = ticket.Id };
            }
        }
        await domainEventDispatcher.DispatchAsync(events).ConfigureAwait(false);
        ticket.ClearDomainEvents();

        ClearDashboardCache();

        return ticket;
    }

    private static async Task ApplySlaPolicyAsync(IRepositoryFacade repo, Ticket ticket)
    {
        var candidates = await repo.SlaPolicies.GetActiveForPriorityAsync(ticket.Priority).ConfigureAwait(false);

        var policy = candidates
            .Where(p => p.ProductId == null || p.ProductId == ticket.ProductId)
            .OrderByDescending(p => p.Specificity)
            .ThenBy(p => p.Id)
            .FirstOrDefault();

        if (policy is null)
        {
            ticket.DueDate ??= FallbackDueDate(ticket.CreatedAt, ticket.Priority);
            return;
        }

        ticket.SlaPolicyId = policy.Id;
        ticket.DueDate ??= policy.GetResolutionDueAt(ticket.CreatedAt);
        ticket.ResponseDueAt ??= policy.GetResponseDueAt(ticket.CreatedAt);
    }

    private static DateTime FallbackDueDate(DateTime baseDate, TicketPriority priority) => priority switch
    {
        TicketPriority.Critical => baseDate.AddHours(4),
        TicketPriority.High => baseDate.AddDays(2).Date.AddHours(16),
        TicketPriority.Medium => baseDate.AddDays(5).Date.AddHours(16),
        TicketPriority.Low => baseDate.AddDays(14).Date.AddHours(16),
        _ => baseDate.AddDays(5).Date.AddHours(16),
    };



    public async Task<bool> UpdateTicketStatusAsync(int ticketId, TicketStatus newStatus, int? actorUserId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        var oldStatus = ticket.Status;
        ticket.ChangeStatus(newStatus, actorUserId);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        var events = ticket.DomainEvents.ToList();
        await domainEventDispatcher.DispatchAsync(events).ConfigureAwait(false);
        ticket.ClearDomainEvents();

        ClearDashboardCache();

        // The remaining code (notification, audit, realtimeNotifier) is handled by DomainEvent handlers.
        // But what if oldStatus == newStatus? The DomainEvents collection will be empty, so DispatchAsync does nothing.

        return true;
    }

    public async Task<bool> UpdateTicketPriorityAsync(int ticketId, TicketPriority newPriority)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        var oldPriority = ticket.Priority;
        ticket.Priority = newPriority;
        ticket.IsPriorityAssessed = true;

        // Recalculate SLA due date based on new priority
        var baseDate = ticket.CreatedAt;
        ticket.DueDate = newPriority switch
        {
            TicketPriority.Critical => baseDate.AddHours(4),
            TicketPriority.High => baseDate.AddDays(2).Date.AddHours(16),
            TicketPriority.Medium => baseDate.AddDays(5).Date.AddHours(16),
            TicketPriority.Low => baseDate.AddDays(14).Date.AddHours(16),
            _ => baseDate.AddDays(5).Date.AddHours(16)
        };
        ticket.IsSlaBreached = false;
        ticket.SlaWarningSent = false;

        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        ClearDashboardCache();

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        if (auditService != null)
        {
            await auditService.LogActionAsync(
                "UpdateTicketPriority",
                "Ticket",
                ticketId.ToString(),
                $"Updated priority from {oldPriority} to {newPriority}",
                null
            ).ConfigureAwait(false);
        }

        return true;
    }

    public async Task<bool> UpdateTicketDatesAsync(int ticketId, DateTime? startDate, DateTime? dueDate, int? actorUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        bool datesChanged = ticket.StartDate != startDate || ticket.DueDate != dueDate;

        ticket.StartDate = startDate;
        ticket.DueDate = dueDate;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        if (datesChanged)
        {
            await notificationService.CreateDatesChangedNotificationAsync(ticketId, actorUserId).ConfigureAwait(false);
        }

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> UpdateAnalyticalNoteAsync(int ticketId, string? analyticalNote, int? actorUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.AnalyticalNote = analyticalNote;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        if (auditService != null)
        {
            await auditService.LogActionAsync(
                "UpdateAnalyticalNote",
                "Ticket",
                ticketId.ToString(),
                "Updated analytical note",
                actorUserId
            ).ConfigureAwait(false);
        }

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdWithIncludesAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        await repo.Tickets.DeleteAsync(ticketId).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("DELETE", "Ticket", ticket.Id.ToString(), $"Deleted ticket: {ticket.Title}").ConfigureAwait(false);

        ClearDashboardCache();

        return true;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var products = await repo.Products.GetAllAsync().ConfigureAwait(false);
        return products.OrderBy(p => p.Name).ToList();
    }

    public async Task<int> GetTotalTicketsCountAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetTotalCountAsync().ConfigureAwait(false);
    }

    public async Task<int> GetOpenTicketsCountAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByStatusAsync(TicketStatus.Open).ConfigureAwait(false);
    }

    public async Task<int> GetCriticalTicketsCountAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByPriorityAsync(TicketPriority.Critical).ConfigureAwait(false);
    }

    public async Task<int> GetUserTicketsCountAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByAuthorIdAsync(userId).ConfigureAwait(false);
    }

    public async Task<List<Ticket>> GetUserTicketsAsync(int userId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetByAuthorIdAsync(userId).ConfigureAwait(false);
        return tickets.ToList();
    }

    public async Task<bool> AssignDeveloperAsync(int ticketId, int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = developerId;
        ticket.FirstRespondedAt ??= DateTime.UtcNow;

        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> UnassignDeveloperAsync(int ticketId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);

        if (ticket is null)
        {
            return false;
        }

        ticket.DeveloperId = null;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<List<Ticket>> GetDeveloperTicketsAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetByDeveloperIdAsync(developerId).ConfigureAwait(false);
        return tickets.ToList();
    }

    public async Task<int> GetDeveloperAssignedCountAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetCountByDeveloperIdAsync(developerId).ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperInProgressCountAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetDeveloperInProgressCountAsync(developerId).ConfigureAwait(false);
    }

    public async Task<int> GetDeveloperCompletedCountAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetDeveloperCompletedCountAsync(developerId).ConfigureAwait(false);
    }

    public async Task<DeveloperDashboardStatsDto> GetDeveloperDashboardStatsAsync(int developerId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var total = await repo.Tickets.GetCountByDeveloperIdAsync(developerId).ConfigureAwait(false);
        var inProgress = await repo.Tickets.GetDeveloperInProgressCountAsync(developerId).ConfigureAwait(false);
        var completed = await repo.Tickets.GetDeveloperCompletedCountAsync(developerId).ConfigureAwait(false);
        return new DeveloperDashboardStatsDto(total, inProgress, completed);
    }

    public async Task<Dictionary<string, int>> GetTicketCountByStatusAsync()
    {
        return await memoryCache.GetOrCreateAsync("TicketCountByStatus", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            await using var repo = repositoryFacadeFactory.Create();
            var counts = await repo.Tickets.GetTicketCountGroupedByStatusAsync().ConfigureAwait(false);
            return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
        }) ?? [];
    }

    public async Task<Dictionary<string, int>> GetTicketCountByPriorityAsync()
    {
        return await memoryCache.GetOrCreateAsync("TicketCountByPriority", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            await using var repo = repositoryFacadeFactory.Create();
            var counts = await repo.Tickets.GetTicketCountGroupedByPriorityAsync().ConfigureAwait(false);
            return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
        }) ?? [];
    }

    public async Task<Dictionary<string, int>> GetTicketCountByTypeAsync()
    {
        return await memoryCache.GetOrCreateAsync("TicketCountByType", async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
            await using var repo = repositoryFacadeFactory.Create();
            var counts = await repo.Tickets.GetTicketCountGroupedByTypeAsync().ConfigureAwait(false);
            return counts.ToDictionary(k => k.Key.ToString(), v => v.Value);
        }) ?? [];
    }

    public async Task<List<(string Login, int Count)>> GetTopDevelopersAsync(int top = 5)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tickets.GetTopDevelopersByResolvedTicketsAsync(top).ConfigureAwait(false);
    }

    public async Task<ExtendedAnalyticsDto> GetExtendedAnalyticsAsync(int days = 30)
    {
        if (days <= 0)
        {
            days = 30;
        }

        await using var repo = repositoryFacadeFactory.Create();
        var allTickets = (await repo.Tickets.GetAllWithIncludesAsync().ConfigureAwait(false)).ToList();
        var allTags = (await repo.Tags.GetAllAsync().ConfigureAwait(false)).ToList();
        var developers = (await repo.Users.GetAllAsync().ConfigureAwait(false))
            .Where(u => u.Role == UserRole.Developer || u.Role == UserRole.Admin)
            .ToList();

        var today = DateTime.UtcNow.Date;
        var startDate = today.AddDays(-days + 1);

        // 1. Daily Trends
        var trends = new List<DailyTicketTrendDto>();
        for (var date = startDate; date <= today; date = date.AddDays(1))
        {
            var targetDate = date;
            var created = allTickets.Count(t => t.CreatedAt.Date == targetDate);
            var resolved = allTickets.Count(t =>
                (t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done) &&
                ((t.DueDate.HasValue ? t.DueDate.Value.Date : t.CreatedAt.Date) == targetDate));

            trends.Add(new DailyTicketTrendDto(
                targetDate,
                targetDate.ToString("dd MMM", System.Globalization.CultureInfo.InvariantCulture),
                created,
                resolved
            ));
        }

        // 2. Developer Workloads
        var devWorkloads = new List<DeveloperWorkloadDto>();
        foreach (var dev in developers)
        {
            var devTickets = allTickets.Where(t => t.DeveloperId == dev.Id).ToList();
            var active = devTickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Done);
            var inProgress = devTickets.Count(t => t.Status == TicketStatus.InProgress || t.Status == TicketStatus.Testing || t.Status == TicketStatus.CodeReview);
            var completed = devTickets.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done);
            var score = (inProgress * 2.0) + (active - inProgress);

            if (active > 0 || completed > 0 || dev.Role == UserRole.Developer)
            {
                devWorkloads.Add(new DeveloperWorkloadDto(
                    dev.Id,
                    dev.Login,
                    active,
                    inProgress,
                    completed,
                    score
                ));
            }
        }
        devWorkloads = devWorkloads.OrderByDescending(d => d.WorkloadScore).ThenByDescending(d => d.AssignedCount).ToList();

        // 3. Product Resolution Performance
        var productPerformances = allTickets
            .Where(t => t.Product != null)
            .GroupBy(t => new { t.Product!.Id, t.Product.Name })
            .Select(g =>
            {
                var total = g.Count();
                var resolvedGroup = g.Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done).ToList();
                var avgHours = resolvedGroup.Count > 0
                    ? resolvedGroup.Average(t =>
                    {
                        var lastDate = t.DueDate ?? (t.Comments.Count > 0 ? t.Comments.Max(c => c.CreatedAt) : t.CreatedAt.AddHours(24));
                        return Math.Max(0.5, (lastDate - t.CreatedAt).TotalHours);
                    })
                    : 0.0;

                return new ProductResolutionPerformanceDto(
                    g.Key.Id,
                    g.Key.Name,
                    total,
                    resolvedGroup.Count,
                    Math.Round(avgHours, 1)
                );
            })
            .OrderByDescending(p => p.TotalTickets)
            .ToList();

        // 4. Tag Distributions
        var tagDistributions = allTags
            .Select(tag =>
            {
                var count = allTickets.Count(t => t.Tags != null && t.Tags.Any(tg => tg.Id == tag.Id));
                return new TagAnalyticsDto(tag.Id, tag.Name, tag.Color, count);
            })
            .Where(t => t.TicketCount > 0)
            .OrderByDescending(t => t.TicketCount)
            .ToList();

        // 5. Type Distributions
        var totalTicketsCount = allTickets.Count;
        var typeDistributions = Enum.GetValues<TicketType>()
            .Select(type =>
            {
                var count = allTickets.Count(t => t.Type == type);
                var pct = totalTicketsCount > 0 ? Math.Round((double)count / totalTicketsCount * 100.0, 1) : 0.0;
                return new TicketTypeAnalyticsDto(type, type.ToString(), count, pct);
            })
            .OrderByDescending(t => t.TicketCount)
            .ToList();

        // 6. KPIs
        var openTicketsCount = allTickets.Count(t => t.Status != TicketStatus.Resolved && t.Status != TicketStatus.Closed && t.Status != TicketStatus.Done);
        var resolvedTicketsCount = allTickets.Count(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done);
        var allResolved = allTickets.Where(t => t.Status == TicketStatus.Resolved || t.Status == TicketStatus.Closed || t.Status == TicketStatus.Done).ToList();
        var overallAvgHours = allResolved.Count > 0
            ? Math.Round(allResolved.Average(t =>
            {
                var lastDate = t.DueDate ?? (t.Comments.Count > 0 ? t.Comments.Max(c => c.CreatedAt) : t.CreatedAt.AddHours(24));
                return Math.Max(0.5, (lastDate - t.CreatedAt).TotalHours);
            }), 1)
            : 0.0;
        var activeDevsCount = devWorkloads.Count(d => d.AssignedCount > 0);
        var resRate = totalTicketsCount > 0
            ? Math.Round((double)resolvedTicketsCount / totalTicketsCount * 100.0, 1)
            : 0.0;

        var kpis = new AnalyticsKpiDto(
            totalTicketsCount,
            openTicketsCount,
            resolvedTicketsCount,
            overallAvgHours,
            activeDevsCount,
            resRate
        );

        return new ExtendedAnalyticsDto(
            kpis,
            trends,
            devWorkloads,
            productPerformances,
            tagDistributions,
            typeDistributions
        );
    }

    public async Task<StatisticsReportDto> GetStatisticsReportAsync(int days, TimeZoneInfo zone)
    {
        ArgumentNullException.ThrowIfNull(zone);

        if (days <= 0)
        {
            days = 30;
        }

        DateOnly LocalDate(DateTime utc) =>
            DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), zone));

        // The period is whole days in the user's zone: today and the days - 1 before it.
        var to = LocalDate(DateTime.UtcNow);
        var from = to.AddDays(-(days - 1));
        bool InPeriod(DateOnly date) => date >= from && date <= to;

        await using var repo = repositoryFacadeFactory.Create();
        var tickets = await repo.Tickets.GetAllWithProductAndTagsAsync().ConfigureAwait(false);
        var firstResolved = await repo.TicketStatusHistory.GetFirstResolvedTimesAsync().ConfigureAwait(false);
        var users = await repo.Users.GetAllWithPersonAsync().ConfigureAwait(false);

        // One extra day on the UTC side covers any zone offset; exact filtering is by the date in the zone below.
        var sinceUtc = from.AddDays(-1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var workLogs = await repo.WorkLogs.GetEntriesSinceAsync(sinceUtc).ConfigureAwait(false);

        // Resolved at = the first change to a done status. Done tickets without such a row (old data the
        // history backfill could not rebuild) have no resolution time and stay out of the period numbers.
        var rows = tickets
            .Select(t =>
            {
                var createdOn = LocalDate(t.CreatedAt);
                DateTime? resolvedAt = IsDone(t.Status) && firstResolved.TryGetValue(t.Id, out var at) ? at : null;
                DateOnly? resolvedOn = resolvedAt is DateTime r && InPeriod(LocalDate(r)) ? LocalDate(r) : null;
                return new StatisticsTicket(
                    t,
                    InPeriod(createdOn) ? createdOn : null,
                    resolvedOn,
                    resolvedOn is null ? null : resolvedAt!.Value - t.CreatedAt);
            })
            .ToList();
        var open = rows.Where(r => !IsDone(r.Ticket.Status)).ToList();

        var snapshot = new StatisticsSnapshotDto(
            open.Count,
            open.Count(r => r.Ticket.Priority == TicketPriority.Critical),
            Enum.GetValues<TicketStatus>().ToDictionary(s => s, s => rows.Count(r => r.Ticket.Status == s)),
            Enum.GetValues<TicketPriority>().ToDictionary(p => p, p => open.Count(r => r.Ticket.Priority == p)),
            Enum.GetValues<TicketType>().ToDictionary(t => t, t => open.Count(r => r.Ticket.Type == t)),
            open.SelectMany(r => r.Ticket.Tags)
                .GroupBy(tag => tag.Id)
                .Select(g => new TagCountDto(g.Key, g.First().Name, g.Count()))
                .OrderByDescending(t => t.Count)
                .ThenBy(t => t.Name, StringComparer.CurrentCultureIgnoreCase)
                .ToList());

        var created = rows.Count(r => r.CreatedOn is not null);
        var resolved = rows.Where(r => r.ResolutionTime is not null).ToList();
        var period = new StatisticsPeriodDto(
            created,
            resolved.Count,
            AverageResolution(resolved),
            created > 0 ? (int)Math.Round(resolved.Count * 100.0 / created, MidpointRounding.AwayFromZero) : null);

        var createdByDay = rows.Where(r => r.CreatedOn is not null).CountBy(r => r.CreatedOn!.Value).ToDictionary();
        var resolvedByDay = resolved.CountBy(r => r.ResolvedOn!.Value).ToDictionary();
        var trend = Enumerable.Range(0, days)
            .Select(i => from.AddDays(i))
            .Select(day => new DailyFlowDto(day, createdByDay.GetValueOrDefault(day), resolvedByDay.GetValueOrDefault(day)))
            .ToList();

        var products = rows
            .Where(r => r.Ticket.Product is not null)
            .GroupBy(r => r.Ticket.Product!.Id)
            .Select(g =>
            {
                var productResolved = g.Where(r => r.ResolutionTime is not null).ToList();
                return new ProductStatisticsDto(
                    g.Key,
                    g.First().Ticket.Product!.Name,
                    g.Count(r => !IsDone(r.Ticket.Status)),
                    g.Count(r => r.CreatedOn is not null),
                    productResolved.Count,
                    AverageResolution(productResolved));
            })
            .Where(p => p.Open > 0 || p.Created > 0 || p.Resolved > 0)
            .OrderBy(p => p.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        var byDeveloper = rows.Where(r => r.Ticket.DeveloperId is not null).ToLookup(r => r.Ticket.DeveloperId!.Value);
        var minutesByUser = workLogs
            .Where(w => InPeriod(LocalDate(w.DateLogged)))
            .GroupBy(w => w.UserId)
            .ToDictionary(g => g.Key, g => g.Sum(w => w.Minutes));

        var team = users
            .Where(u => u.Role is UserRole.Developer or UserRole.Admin)
            .Select(u =>
            {
                var assigned = byDeveloper[u.Id].ToList();
                var name = $"{u.Person?.FirstName} {u.Person?.LastName}".Trim();
                return (User: u, Row: new DeveloperStatisticsDto(
                    u.Id,
                    u.Login,
                    string.IsNullOrEmpty(name) ? u.Login : name,
                    assigned.Count(r => IsInProgress(r.Ticket.Status)),
                    assigned.Count(r => !IsDone(r.Ticket.Status)),
                    assigned.Count(r => r.ResolutionTime is not null),
                    minutesByUser.GetValueOrDefault(u.Id)));
            })

            // Active developers are always listed (an empty row is information too); admins and
            // deactivated developers only when they have something to show.
            .Where(x => (x.User.Role == UserRole.Developer && x.User.IsActive) || !x.Row.IsIdle)
            .Select(x => x.Row)
            .OrderByDescending(r => r.ClosedInPeriod)
            .ThenByDescending(r => r.Workload)
            .ThenBy(r => r.DisplayName, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new StatisticsReportDto(days, from, to, snapshot, period, trend, products, team);
    }

    private static bool IsDone(TicketStatus status) =>
        status is TicketStatus.Resolved or TicketStatus.Closed or TicketStatus.Done;

    private static bool IsInProgress(TicketStatus status) =>
        status is TicketStatus.InProgress or TicketStatus.CodeReview or TicketStatus.Testing;

    private static TimeSpan? AverageResolution(IReadOnlyCollection<StatisticsTicket> resolved) =>
        resolved.Count == 0
            ? null
            : TimeSpan.FromTicks((long)resolved.Average(r => Math.Max(0, r.ResolutionTime!.Value.Ticks)));

    /// <summary>
    /// A ticket with its period facts: the local day it was created and resolved on (<c>null</c> outside
    /// the period) and, when resolved in the period, the time it took.
    /// </summary>
    private sealed record StatisticsTicket(Ticket Ticket, DateOnly? CreatedOn, DateOnly? ResolvedOn, TimeSpan? ResolutionTime);
}


