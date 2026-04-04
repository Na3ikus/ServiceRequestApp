using System.ComponentModel.DataAnnotations;

namespace ServiceDeskSystem.Infrastructure.Email;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int Port { get; set; } = 587;

    public bool UseSsl { get; set; }

    public bool UseStartTls { get; set; } = true;

    public bool UseAuthentication { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    [EmailAddress]
    public string FromEmail { get; set; } = string.Empty;

    public string FromName { get; set; } = "ServiceDeskSystem";

    [Range(5, 120)]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1, 5)]
    public int MaxRetryAttempts { get; set; } = 3;
}
