using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;

namespace ServiceDeskSystem.Application.Services.Sla;

public interface ISlaPolicyService
{
    Task<IReadOnlyList<SlaPolicy>> GetAllPoliciesAsync();

    Task<SlaPolicy?> GetPolicyByIdAsync(int id);

    Task<SlaPolicy> CreatePolicyAsync(SlaPolicy policy, int? currentUserId = null);

    Task<SlaPolicy?> UpdatePolicyAsync(SlaPolicy policy, int? currentUserId = null);

    Task<bool> DeletePolicyAsync(int id, int? currentUserId = null);

    Task<bool> SetActiveAsync(int id, bool isActive, int? currentUserId = null);

    Task<SlaPolicy?> ResolvePolicyAsync(TicketPriority priority, int? productId);

    Task<bool> ApplyPolicyAsync(Ticket ticket, bool overrideExistingDueDate = false);

    Task<int> ReapplyToOpenTicketsAsync(int? currentUserId = null);
}
