using System.Globalization;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Application.Services.Realtime;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Tags;

public sealed class TagService(
    IRepositoryFacadeFactory repositoryFacadeFactory,
    IRealtimeNotifier realtimeNotifier,
    IAuditService? auditService = null) : ITagService
{
    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tags.GetAllWithTicketsAsync().ConfigureAwait(false);
    }

    public async Task<Tag?> GetTagByIdAsync(int id)
    {
        await using var repo = repositoryFacadeFactory.Create();
        return await repo.Tags.GetByIdWithTicketsAsync(id).ConfigureAwait(false);
    }

    public async Task<Tag> CreateTagAsync(string name, string color, int? currentUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        await using var repo = repositoryFacadeFactory.Create();
        var tag = new Tag
        {
            Name = name.Trim(),
            Color = string.IsNullOrWhiteSpace(color) ? "#3B82F6" : color.Trim(),
        };

        await repo.Tags.CreateAsync(tag).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("CREATE_TAG", "Tag", tag.Id.ToString(CultureInfo.InvariantCulture), $"Created tag '{tag.Name}'", currentUserId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return tag;
    }

    public async Task<Tag?> UpdateTagAsync(int id, string name, string color, int? currentUserId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        await using var repo = repositoryFacadeFactory.Create();
        var tag = await repo.Tags.GetByIdAsync(id).ConfigureAwait(false);
        if (tag is null)
        {
            return null;
        }

        tag.Name = name.Trim();
        tag.Color = string.IsNullOrWhiteSpace(color) ? "#3B82F6" : color.Trim();

        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("UPDATE_TAG", "Tag", tag.Id.ToString(CultureInfo.InvariantCulture), $"Updated tag '{tag.Name}'", currentUserId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return tag;
    }

    public async Task<bool> DeleteTagAsync(int id, int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var tag = await repo.Tags.GetByIdAsync(id).ConfigureAwait(false);
        if (tag is null)
        {
            return false;
        }

        await repo.Tags.DeleteAsync(id).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("DELETE_TAG", "Tag", id.ToString(CultureInfo.InvariantCulture), $"Deleted tag '{tag.Name}'", currentUserId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> AssignTagToTicketAsync(int ticketId, int tagId, int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdWithIncludesAsync(ticketId).ConfigureAwait(false);
        if (ticket is null)
        {
            return false;
        }

        var tag = await repo.Tags.GetByIdAsync(tagId).ConfigureAwait(false);
        if (tag is null)
        {
            return false;
        }

        if (ticket.Tags.Any(t => t.Id == tagId))
        {
            return true;
        }

        ticket.Tags.Add(tag);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("ASSIGN_TAG", "Ticket", ticketId.ToString(CultureInfo.InvariantCulture), $"Assigned tag '{tag.Name}' to ticket {ticketId}", currentUserId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }

    public async Task<bool> RemoveTagFromTicketAsync(int ticketId, int tagId, int? currentUserId = null)
    {
        await using var repo = repositoryFacadeFactory.Create();
        var ticket = await repo.Tickets.GetByIdWithIncludesAsync(ticketId).ConfigureAwait(false);
        if (ticket is null)
        {
            return false;
        }

        var tagToRemove = ticket.Tags.FirstOrDefault(t => t.Id == tagId);
        if (tagToRemove is null)
        {
            return true;
        }

        ticket.Tags.Remove(tagToRemove);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await auditService.LogActionSafeAsync("REMOVE_TAG", "Ticket", ticketId.ToString(CultureInfo.InvariantCulture), $"Removed tag '{tagToRemove.Name}' from ticket {ticketId}", currentUserId).ConfigureAwait(false);
        await realtimeNotifier.NotifyTicketsChangedAsync().ConfigureAwait(false);

        return true;
    }
}
