using Microsoft.EntityFrameworkCore;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Enums;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;

namespace ServiceDeskSystem.Infrastructure.Data.Repository
{
    public sealed class SlaPolicyRepository : TemplateRepository<SlaPolicy>, ISlaPolicyRepository
    {
        public SlaPolicyRepository(BugTrackerDbContext context)
            : base(context)
        {
        }

        protected override DbSet<SlaPolicy> DbSet => this.Context.SlaPolicies;

        public async Task<IEnumerable<SlaPolicy>> GetAllWithProductAsync()
        {
            return await this.Context.SlaPolicies
                .Include(p => p.Product)
                .OrderByDescending(p => p.Priority)
                .ThenBy(p => p.ProductId == null ? 0 : 1)
                .ThenBy(p => p.Name)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<SlaPolicy?> GetByIdWithProductAsync(int id)
        {
            return await this.Context.SlaPolicies
                .Include(p => p.Product)
                .FirstOrDefaultAsync(p => p.Id == id)
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<SlaPolicy>> GetActiveForPriorityAsync(TicketPriority priority)
        {
            return await this.Context.SlaPolicies
                .Where(p => p.IsActive && p.Priority == priority)
                .ToListAsync()
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsForScopeAsync(TicketPriority priority, int? productId, int? excludeId = null)
        {
            return await this.Context.SlaPolicies
                .AnyAsync(p => p.Priority == priority
                    && p.ProductId == productId
                    && (excludeId == null || p.Id != excludeId))
                .ConfigureAwait(false);
        }
    }
}
