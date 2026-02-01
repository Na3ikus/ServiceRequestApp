namespace ServiceDeskSystem.Components.Common;

/// <summary>
/// Types of toast notifications.
/// </summary>
public enum ToastType
{
    /// <summary>Informational message.</summary>
    Info,

    /// <summary>Success message.</summary>
    Success,

    /// <summary>Warning message.</summary>
    Warning,

    /// <summary>Error message.</summary>
    Error,
}

/// <summary>
/// Represents a toast notification message.
/// </summary>
public sealed class ToastMessage
{
    /// <summary>Gets the unique identifier.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>Gets the message text.</summary>
    required public string Message { get; init; }

    /// <summary>Gets the toast type.</summary>
    public ToastType Type { get; init; }

    /// <summary>Gets or sets a value indicating whether the toast is hiding.</summary>
    public bool IsHiding { get; set; }
}