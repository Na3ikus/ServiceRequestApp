using Microsoft.AspNetCore.Hosting;
using ServiceDeskSystem.Application.Services.Audit;
using ServiceDeskSystem.Domain.Entities;
using ServiceDeskSystem.Domain.Interfaces;

namespace ServiceDeskSystem.Application.Services.Attachments;

public class AttachmentService : IAttachmentService
{
    private readonly IRepositoryFacadeFactory _repositoryFacadeFactory;
    private readonly IAuditService _auditService;
    private readonly IWebHostEnvironment _env;

    private const long MaxFileSize = 15 * 1024 * 1024; // 15 MB
    private static readonly string[] AllowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".txt", ".log", ".json", ".csv", ".docx", ".xlsx", ".zip", ".rar", ".7z" };

    public AttachmentService(IRepositoryFacadeFactory repositoryFacadeFactory, IAuditService auditService, IWebHostEnvironment env)
    {
        this._repositoryFacadeFactory = repositoryFacadeFactory;
        this._auditService = auditService;
        this._env = env;
    }

    public async Task<(bool Success, string? ErrorMessage)> DeleteAttachmentAsync(int attachmentId, int currentUserId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();

        var attachment = await repo.Attachments.GetByIdAsync(attachmentId).ConfigureAwait(false);
        if (attachment is null)
        {
            return (false, "Attachment not found.");
        }

        var user = await repo.Users.GetByIdAsync(currentUserId).ConfigureAwait(false);
        if (user is null)
        {
            return (false, "User not found.");
        }

        if (attachment.UploadedById != currentUserId && user.Role != Domain.Enums.UserRole.Admin && user.Role != Domain.Enums.UserRole.Developer)
        {
            return (false, "Forbidden: You don't have permission to delete this attachment.");
        }

        var physicalPath = Path.Combine(this._env.WebRootPath, attachment.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (File.Exists(physicalPath))
        {
            try
            {
                File.Delete(physicalPath);
            }
            catch
            {
                // Log exception if needed, but proceed to delete from DB
            }
        }

        await repo.Attachments.DeleteAsync(attachmentId).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await this._auditService.LogActionSafeAsync("ATTACHMENT_DELETED", "Ticket", attachment.TicketId.ToString(), $"Deleted attachment: {attachment.FileName}", currentUserId).ConfigureAwait(false);

        return (true, null);
    }

    public async Task<Attachment?> GetAttachmentByIdAsync(int attachmentId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();
        return await repo.Attachments.GetByIdAsync(attachmentId).ConfigureAwait(false);
    }

    public async Task<IEnumerable<Attachment>> GetAttachmentsByTicketIdAsync(int ticketId)
    {
        await using var repo = this._repositoryFacadeFactory.Create();
        return await repo.Attachments.GetByTicketIdAsync(ticketId).ConfigureAwait(false);
    }

    public async Task<(bool Success, string? ErrorMessage, Attachment? Attachment)> UploadAttachmentAsync(int ticketId, string fileName, string contentType, Stream contentStream, int currentUserId, int? commentId = null)
    {
        if (contentStream.Length > MaxFileSize)
        {
            return (false, $"File size exceeds the limit of {MaxFileSize / 1024 / 1024} MB.", null);
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
        {
            return (false, "File type is not allowed.", null);
        }

        await using var repo = this._repositoryFacadeFactory.Create();

        var ticket = await repo.Tickets.GetByIdAsync(ticketId).ConfigureAwait(false);
        if (ticket is null)
        {
            return (false, "Ticket not found.", null);
        }

        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var relativeDir = $"uploads/tickets/{ticketId}";
        var physicalDir = Path.Combine(this._env.WebRootPath, "uploads", "tickets", ticketId.ToString());
        var physicalPath = Path.Combine(physicalDir, storedFileName);
        var relativePath = $"{relativeDir}/{storedFileName}";

        try
        {
            Directory.CreateDirectory(physicalDir);

            await using var fileStream = new FileStream(physicalPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
            await contentStream.CopyToAsync(fileStream).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            return (false, $"Failed to save file: {ex.Message}", null);
        }

        var attachment = new Attachment
        {
            FileName = fileName,
            StoredFileName = storedFileName,
            FilePath = relativePath,
            FileSize = contentStream.Length,
            ContentType = contentType,
            UploadedAt = DateTime.UtcNow,
            UploadedById = currentUserId,
            TicketId = ticketId,
            CommentId = commentId
        };

        await repo.Attachments.CreateAsync(attachment).ConfigureAwait(false);
        await repo.UnitOfWork.SaveChangesAsync().ConfigureAwait(false);

        await this._auditService.LogActionSafeAsync("ATTACHMENT_ADDED", "Ticket", ticketId.ToString(), $"Uploaded attachment: {fileName}", currentUserId).ConfigureAwait(false);

        return (true, null, attachment);
    }
}
