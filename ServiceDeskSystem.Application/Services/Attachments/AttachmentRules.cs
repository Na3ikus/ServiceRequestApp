namespace ServiceDeskSystem.Application.Services.Attachments;

/// <summary>
/// Upload limits shared by <see cref="AttachmentService"/> and the pages that pick files,
/// so a file accepted in the browser is not refused by the server.
/// </summary>
public static class AttachmentRules
{
    /// <summary>Largest file accepted, in bytes (10 MB).</summary>
    public const long MaxFileSize = 10 * 1024 * 1024;

    /// <summary>Most files that can be attached while creating a ticket.</summary>
    public const int MaxFilesOnCreate = 5;

    /// <summary>Gets the accepted file extensions, lowercase with the dot.</summary>
    public static IReadOnlyList<string> AllowedExtensions { get; } =
        [".png", ".jpg", ".jpeg", ".gif", ".webp", ".pdf", ".txt", ".log", ".json", ".csv", ".docx", ".xlsx", ".zip", ".rar", ".7z"];

    /// <summary>Gets the value for an <c>accept</c> attribute of a file input.</summary>
    public static string Accept { get; } = string.Join(',', AllowedExtensions);

    /// <summary>Checks the extension of <paramref name="fileName"/> against <see cref="AllowedExtensions"/>.</summary>
    public static bool IsAllowedType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension.Length > 0 && AllowedExtensions.Contains(extension);
    }
}
