using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
namespace ServiceDeskSystem.Infrastructure.Data.Repository;
public class ContactInfoRepository(BugTrackerDbContext context) : TemplateRepository<ContactInfo>(context), IContactInfoRepository {
    protected override DbSet<ContactInfo> DbSet => this.Context.ContactInfos;

    public async Task<bool> ExistsByEmailAsync(string email, int emailContactTypeId)
    {
        return await this.Context.ContactInfos
            .AnyAsync(ci => ci.ContactTypeId == emailContactTypeId && ci.Value == email)
            .ConfigureAwait(false);
    }
}
