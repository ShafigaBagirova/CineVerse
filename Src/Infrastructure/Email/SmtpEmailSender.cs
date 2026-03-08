using Application.Auth.Options;
using Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Infrastructure.Email;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string toEmail,
        string subject,
        string htmlBody,
        string? textBody = null,
        CancellationToken ct = default)
    {
        if (!_options.SendEmails)
            return;

        if (string.IsNullOrWhiteSpace(toEmail))
            return;

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_options.FromName, _options.FromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder();

        if (!string.IsNullOrWhiteSpace(htmlBody))
        {
            bodyBuilder.HtmlBody = htmlBody;

            if (!string.IsNullOrWhiteSpace(textBody))
                bodyBuilder.TextBody = textBody;
        }
        else
        {
            bodyBuilder.TextBody = textBody ?? string.Empty;
        }

        message.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();

        var secureSocket = _options.EnableSsl
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.Auto;

        await smtp.ConnectAsync(_options.SmtpHost, _options.SmtpPort, secureSocket, ct);

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            await smtp.AuthenticateAsync(_options.UserName, _options.Password, ct);
        }

        await smtp.SendAsync(message, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}