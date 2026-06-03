using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
namespace ServiceDeskSystem.Infrastructure.Data.Repository;
public class ContactTypeRepository(BugTrackerDbContext context) : TemplateRepository<ContactType>(context), IContactTypeRepository {
    protected override DbSet<ContactType> DbSet => this.Context.ContactTypes;
}
