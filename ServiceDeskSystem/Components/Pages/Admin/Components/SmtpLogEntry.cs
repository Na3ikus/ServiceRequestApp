namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// One command in the SMTP terminal log and its result (<c>null</c> while it runs).
/// </summary>
public sealed class SmtpLogEntry
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SmtpLogEntry"/> class.
    /// </summary>
    /// <param name="command">The command line without the <c>$</c> prompt.</param>
    public SmtpLogEntry(string command)
    {
        this.Command = command;
    }

    /// <summary>
    /// Gets the command line without the <c>$</c> prompt, e.g. <c>smtp check</c>.
    /// </summary>
    public string Command { get; }

    /// <summary>
    /// Gets a value indicating whether the command succeeded; <c>null</c> while it runs.
    /// </summary>
    public bool? IsSuccess { get; private set; }

    /// <summary>
    /// Gets the result text shown after <c>ok:</c> or <c>err:</c>.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// Records the result of the command.
    /// </summary>
    /// <param name="isSuccess">Whether the command succeeded.</param>
    /// <param name="message">The result text.</param>
    public void Complete(bool isSuccess, string message)
    {
        this.IsSuccess = isSuccess;
        this.Message = message;
    }
}
