using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Tags;

public interface ITagService
{
    Task<IEnumerable<Tag>> GetAllTagsAsync();

    Task<Tag?> GetTagByIdAsync(int id);

    Task<Tag> CreateTagAsync(string name, string color, int? currentUserId = null);

    Task<Tag?> UpdateTagAsync(int id, string name, string color, int? currentUserId = null);

    Task<bool> DeleteTagAsync(int id, int? currentUserId = null);

    Task<bool> AssignTagToTicketAsync(int ticketId, int tagId, int? currentUserId = null);

    Task<bool> RemoveTagFromTicketAsync(int ticketId, int tagId, int? currentUserId = null);
}
