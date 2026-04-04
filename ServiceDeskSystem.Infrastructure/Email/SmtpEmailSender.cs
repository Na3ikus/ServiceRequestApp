using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using ServiceDeskSystem.Domain.Interfaces;
using System.Net.Sockets;

namespace ServiceDeskSystem.Infrastructure.Email;

public sealed class SmtpEmailSender(IOptions<SmtpOptions> options, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, string? textBody = null, CancellationToken cancellationToken = default)
    {
        var smtpOptions = options.Value;
        EnsureSmtpEnabledAndConfigured(smtpOptions);

        if (string.IsNullOrWhiteSpace(toEmail))
        {
            throw new ArgumentException("Recipient email is required.", nameof(toEmail));
        }

        if (!System.Net.Mail.MailAddress.TryCreate(toEmail, out _))
        {
            throw new ArgumentException("Recipient email is invalid.", nameof(toEmail));
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtpOptions.FromName, smtpOptions.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = string.IsNullOrWhiteSpace(textBody) ? htmlBody : textBody,
        };

        message.Body = bodyBuilder.ToMessageBody();

        await SendWithRetriesAsync(message, smtpOptions, cancellationToken).ConfigureAwait(false);
    }

    public async Task<(bool IsSuccess, string Message)> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        var smtpOptions = options.Value;

        if (!smtpOptions.Enabled)
        {
            return (false, "SMTP is disabled in configuration.");
        }

        try
        {
            EnsureSmtpEnabledAndConfigured(smtpOptions);

            using var client = CreateClient(smtpOptions);
            await client.ConnectAsync(smtpOptions.Host, smtpOptions.Port, GetSecureSocketOptions(smtpOptions), cancellationToken).ConfigureAwait(false);

            if (smtpOptions.UseAuthentication)
            {
                await client.AuthenticateAsync(smtpOptions.Username, smtpOptions.Password, cancellationToken).ConfigureAwait(false);
            }

            await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
            return (true, "SMTP connection and authentication succeeded.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "SMTP health check failed.");
            return (false, ex.Message);
        }
    }

    private async Task SendWithRetriesAsync(MimeMessage message, SmtpOptions smtpOptions, CancellationToken cancellationToken)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= smtpOptions.MaxRetryAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using var client = CreateClient(smtpOptions);
                await client.ConnectAsync(smtpOptions.Host, smtpOptions.Port, GetSecureSocketOptions(smtpOptions), cancellationToken).ConfigureAwait(false);

                if (smtpOptions.UseAuthentication)
                {
                    await client.AuthenticateAsync(smtpOptions.Username, smtpOptions.Password, cancellationToken).ConfigureAwait(false);
                }

                await client.SendAsync(message, cancellationToken).ConfigureAwait(false);
                await client.DisconnectAsync(true, cancellationToken).ConfigureAwait(false);
                return;
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < smtpOptions.MaxRetryAttempts)
            {
                lastException = ex;
                var delayMs = (int)Math.Pow(2, attempt) * 500;
                logger.LogWarning(ex, "SMTP send attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelayMs} ms.", attempt, smtpOptions.MaxRetryAttempts, delayMs);
                await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                lastException = ex;
                logger.LogError(ex, "SMTP send failed on attempt {Attempt}/{MaxAttempts}.", attempt, smtpOptions.MaxRetryAttempts);
                break;
            }
        }

        throw new InvalidOperationException("Failed to send email via SMTP.", lastException);
    }

    private static MailKit.Net.Smtp.SmtpClient CreateClient(SmtpOptions smtpOptions)
    {
        return new MailKit.Net.Smtp.SmtpClient
        {
            Timeout = smtpOptions.TimeoutSeconds * 1000,
        };
    }

    private static SecureSocketOptions GetSecureSocketOptions(SmtpOptions smtpOptions)
    {
        if (smtpOptions.UseSsl)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        if (smtpOptions.UseStartTls)
        {
            return SecureSocketOptions.StartTls;
        }

        return SecureSocketOptions.None;
    }

    private static bool IsTransient(Exception exception)
    {
        return exception switch
        {
            SocketException => true,
            IOException => true,
            MailKit.ServiceNotConnectedException => true,
            MailKit.ServiceNotAuthenticatedException => false,
            MailKit.CommandException => false,
            _ => false,
        };
    }

    private static void EnsureSmtpEnabledAndConfigured(SmtpOptions smtpOptions)
    {
        if (!smtpOptions.Enabled)
        {
            throw new InvalidOperationException("SMTP is disabled. Enable 'Smtp:Enabled' in configuration.");
        }

        if (string.IsNullOrWhiteSpace(smtpOptions.Host))
        {
            throw new InvalidOperationException("SMTP host is required.");
        }

        if (string.IsNullOrWhiteSpace(smtpOptions.FromEmail))
        {
            throw new InvalidOperationException("SMTP from address is required.");
        }

        if (smtpOptions.UseAuthentication)
        {
            if (string.IsNullOrWhiteSpace(smtpOptions.Username))
            {
                throw new InvalidOperationException("SMTP username is required when authentication is enabled.");
            }

            if (string.IsNullOrWhiteSpace(smtpOptions.Password))
            {
                throw new InvalidOperationException("SMTP password is required when authentication is enabled.");
            }
        }
    }
}
