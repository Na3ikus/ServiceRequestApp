using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;
using ServiceDeskSystem.Infrastructure.Data.Repository.Templates;
using Microsoft.EntityFrameworkCore;
namespace ServiceDeskSystem.Infrastructure.Data.Repository;
public class PersonRepository(BugTrackerDbContext context) : TemplateRepository<Person>(context), IPersonRepository {
    protected override DbSet<Person> DbSet => this.Context.People;
}
