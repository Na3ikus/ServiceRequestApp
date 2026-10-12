namespace ServiceDeskSystem.Components.Pages.Admin.Components;

/// <summary>
/// A test email requested from the SMTP section.
/// </summary>
#pragma warning disable SA1313 // Positional record parameters become PascalCase public properties.
public sealed record SmtpTestEmail(string To, string Subject);
#pragma warning restore SA1313
