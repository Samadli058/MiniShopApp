using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using NotificationService.API.Models;

namespace NotificationService.API.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string body)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(
                _settings.SenderEmail,
                _settings.SenderName),
            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var client = new SmtpClient(
            _settings.Host,
            _settings.Port)
        {
            Credentials = new NetworkCredential(
                _settings.SenderEmail,
                _settings.Password),
            EnableSsl = true
        };

        await client.SendMailAsync(message);
    }
}
