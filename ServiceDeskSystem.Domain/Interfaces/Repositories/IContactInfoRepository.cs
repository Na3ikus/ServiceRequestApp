using ServiceDeskSystem.Domain.Entities;
namespace ServiceDeskSystem.Domain.Interfaces;
public interface IContactInfoRepository : IRepository<ContactInfo> {
    Task<bool> ExistsByEmailAsync(string email, int emailContactTypeId);
}
