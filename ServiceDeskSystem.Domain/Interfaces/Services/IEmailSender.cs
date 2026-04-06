namespace ServiceDeskSystem.Domain.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default);

    Task<(bool IsSuccess, string Message)> CheckConnectionAsync(CancellationToken cancellationToken = default);
}
