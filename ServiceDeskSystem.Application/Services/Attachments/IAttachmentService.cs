using ServiceDeskSystem.Domain.Entities;

namespace ServiceDeskSystem.Application.Services.Attachments;

public interface IAttachmentService
{
    Task<(bool Success, string? ErrorMessage, Attachment? Attachment)> UploadAttachmentAsync(int ticketId, string fileName, string contentType, Stream contentStream, int currentUserId, int? commentId = null);
    Task<IEnumerable<Attachment>> GetAttachmentsByTicketIdAsync(int ticketId);
    Task<Attachment?> GetAttachmentByIdAsync(int attachmentId);
    Task<(bool Success, string? ErrorMessage)> DeleteAttachmentAsync(int attachmentId, int currentUserId);
}
