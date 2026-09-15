using System.Globalization;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Sla;

public sealed class SlaPolicyService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    IRealtimeNotifier realtimeNotifier,
    IAuditService? auditService = null) : ISlaPolicyService
{
    public async Task<IReadOnlyList<SlaPolicy>> GetAllPoliciesAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        var policies = await repo.SlaPolicies.GetAllWithProductAsync().ConfigureAwait(false);
        return policies.ToList();
    }

    public async Task<SlaPolicy?> GetPolicyByIdAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.SlaPolicies.GetByIdWithProductAsync(id).ConfigureAwait(false);
    }

    public async Task<SlaPolicy> CreatePolicyAsync(SlaPolicy policy, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(policy);
        Validate(policy);

        await using var repo = repositoryFacadeFactory.Create();

        if (await repo.SlaPolicies.ExistsForScopeAsync(policy.Priority, policy.ProductId).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"An SLA policy for priority '{policy.Priority}' and this product already exists.");
        }

        policy.Name = policy.Name.Trim();
        policy.WarningThresholdPercent = SlaPolicy.NormalizeThreshold(policy.WarningThresholdPercent);
        policy.CreatedAt = DateTime.UtcNow;
        policy.UpdatedAt = null;

        await repo.SlaPolicies.CreateAsync(policy).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync(
            "CREATE_SLA_POLICY",
            nameof(SlaPolicy),
            policy.Id.ToString(CultureInfo.InvariantCulture),
            Describe(policy),
            currentUserId).ConfigureAwait(false);

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        return policy;
    }

    public async Task<SlaPolicy?> UpdatePolicyAsync(SlaPolicy policy, int? currentUserId = null)
    {
        ArgumentNullException.ThrowIfNull(policy);
        Validate(policy);

        await using var repo = repositoryFacadeFactory.Create();
        var existing = await repo.SlaPolicies.GetByIdAsync(policy.Id).ConfigureAwait(false);
        if (existing is null)
        {
            return null;
        }

        if (await repo.SlaPolicies.ExistsForScopeAsync(policy.Priority, policy.ProductId, policy.Id).ConfigureAwait(false))
        {
            throw new InvalidOperationException($"An SLA policy for priority '{policy.Priority}' and this product already exists.");
        }

        existing.Name = policy.Name.Trim();
        existing.Description = policy.Description;
        existing.Priority = policy.Priority;
        existing.ProductId = policy.ProductId;
        existing.ResponseMinutes = policy.ResponseMinutes;
        existing.ResolutionMinutes = policy.ResolutionMinutes;
        existing.WarningThresholdPercent = SlaPolicy.NormalizeThreshold(policy.WarningThresholdPercent);
        existing.EscalateOnBreach = policy.EscalateOnBreach;
        existing.IsActive = policy.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync(
            "UPDATE_SLA_POLICY",
            nameof(SlaPolicy),
            existing.Id.ToString(CultureInfo.InvariantCulture),
            Describe(existing),
            currentUserId).ConfigureAwait(false);

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        return existing;
    }

    public async Task<bool> DeletePolicyAsync(int id, int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var policy = await repo.SlaPolicies.GetByIdAsync(id).ConfigureAwait(false);
        if (policy is null)
        {
            return false;
        }

        await repo.SlaPolicies.DeleteAsync(id).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync(
            "DELETE_SLA_POLICY",
            nameof(SlaPolicy),
            id.ToString(CultureInfo.InvariantCulture),
            $"Deleted SLA policy '{policy.Name}'",
            currentUserId).ConfigureAwait(false);

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<bool> SetActiveAsync(int id, bool isActive, int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var policy = await repo.SlaPolicies.GetByIdAsync(id).ConfigureAwait(false);
        if (policy is null || policy.IsActive == isActive)
        {
            return false;
        }

        policy.IsActive = isActive;
        policy.UpdatedAt = DateTime.UtcNow;
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync(
            isActive ? "ENABLE_SLA_POLICY" : "DISABLE_SLA_POLICY",
            nameof(SlaPolicy),
            id.ToString(CultureInfo.InvariantCulture),
            $"SLA policy '{policy.Name}' {(isActive ? "enabled" : "disabled")}",
            currentUserId).ConfigureAwait(false);

        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        return true;
    }

    public async Task<SlaPolicy?> ResolvePolicyAsync(TicketPriority priority, int? productId)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var candidates = await repo.SlaPolicies.GetActiveForPriorityAsync(priority).ConfigureAwait(false);
        return SelectBest(candidates, productId);
    }

    public async Task<bool> ApplyPolicyAsync(Ticket ticket, bool overrideExistingDueDate = false)
    {
        ArgumentNullException.ThrowIfNull(ticket);

        var policy = await this.ResolvePolicyAsync(ticket.Priority, ticket.ProductId).ConfigureAwait(false);
        return ApplyToTicket(ticket, policy, overrideExistingDueDate);
    }

    public async Task<int> ReapplyToOpenTicketsAsync(int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var policies = (await repo.SlaPolicies.GetAllWithProductAsync().ConfigureAwait(false))
            .Where(p => p.IsActive)
            .ToList();

        var tickets = await repo.Tickets.GetActiveTicketsForSlaAsync().ConfigureAwait(false);
        var updated = 0;

        foreach (var ticket in tickets)
        {
            var matching = SelectBest(policies.Where(p => p.Priority == ticket.Priority), ticket.ProductId);
            if (ApplyToTicket(ticket, matching, overrideExistingDueDate: true))
            {
                updated++;
            }
        }

        if (updated > 0)
        {
            await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

            await auditService.LogActionSafeAsync(
                "REAPPLY_SLA_POLICIES",
                nameof(SlaPolicy),
                "*",
                $"Recalculated SLA deadlines for {updated} open ticket(s)",
                currentUserId).ConfigureAwait(false);

            await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);
        }

        return updated;
    }

    private static SlaPolicy? SelectBest(IEnumerable<SlaPolicy> candidates, int? productId)
    {
        return candidates
            .Where(p => p.IsActive && (p.ProductId == null || p.ProductId == productId))
            .OrderByDescending(p => p.Specificity)
            .ThenBy(p => p.Id)
            .FirstOrDefault();
    }

    private static bool ApplyToTicket(Ticket ticket, SlaPolicy? policy, bool overrideExistingDueDate)
    {
        if (policy is null)
        {
            return false;
        }

        var baseDate = ticket.CreatedAt == default ? DateTime.UtcNow : ticket.CreatedAt;
        var resolutionDueAt = policy.GetResolutionDueAt(baseDate);
        var responseDueAt = policy.GetResponseDueAt(baseDate);

        var changed = ticket.SlaPolicyId != policy.Id;
        ticket.SlaPolicyId = policy.Id;

        if (overrideExistingDueDate || !ticket.DueDate.HasValue)
        {
            changed |= ticket.DueDate != resolutionDueAt;
            ticket.DueDate = resolutionDueAt;
        }

        if (overrideExistingDueDate || !ticket.ResponseDueAt.HasValue)
        {
            changed |= ticket.ResponseDueAt != responseDueAt;
            ticket.ResponseDueAt = responseDueAt;
        }

        if (changed && ticket.DueDate.HasValue && DateTime.UtcNow <= ticket.DueDate.Value)
        {
            ticket.IsSlaBreached = false;
            ticket.SlaWarningSent = false;
        }

        return changed;
    }

    private static void Validate(SlaPolicy policy)
    {
        if (string.IsNullOrWhiteSpace(policy.Name))
        {
            throw new ArgumentException("SLA policy name is required.", nameof(policy));
        }

        if (policy.ResponseMinutes <= 0)
        {
            throw new ArgumentException("Response time must be greater than zero.", nameof(policy));
        }

        if (policy.ResolutionMinutes <= 0)
        {
            throw new ArgumentException("Resolution time must be greater than zero.", nameof(policy));
        }

        if (policy.ResponseMinutes > policy.ResolutionMinutes)
        {
            throw new ArgumentException("Response time cannot exceed resolution time.", nameof(policy));
        }
    }

    private static string Describe(SlaPolicy policy) =>
        $"'{policy.Name}' — {policy.Priority}, response {SlaPolicy.FormatMinutes(policy.ResponseMinutes)}, " +
        $"resolution {SlaPolicy.FormatMinutes(policy.ResolutionMinutes)}, warn at {policy.WarningThresholdPercent}%";
}
