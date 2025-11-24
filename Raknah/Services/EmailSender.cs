using Microsoft.Extensions.Options;
using Raknah.Setting;
using System.Net;
using System.Net.Mail;
namespace Raknah.Services;

public class EmailSender(IOptions<SMTPOptions> emailOptions, ILogger<EmailSender> logger) : IEmailSendar
{
    public async Task SendEmailAsync(string[] emails, string subject, string? htmlMessage, params string[] filePaths)
    {
        var message = GetMailMessage(subject, htmlMessage);

        foreach (var email in emails)
            message.To.Add(email);

        foreach (var path in filePaths)
        {
            var attachment = new Attachment(path);
            message.Attachments.Add(attachment);
        }

        var smtpClient = GetSmtpClient();

        await smtpClient.SendMailAsync(message);

        smtpClient.Dispose();
    }
    public async Task SendEmailAsync(string email, string subject, string? htmlMessage, params string[] filePaths)
    {
        try
        {
            var message = GetMailMessage(subject, htmlMessage);
            foreach (var path in filePaths)
            {
                var attachment = new Attachment(path);
                message.Attachments.Add(attachment);
            }
            message.To.Add(email);
            var smtpClient = GetSmtpClient();
            await smtpClient.SendMailAsync(message);

            smtpClient.Dispose();

        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private MailMessage GetMailMessage(string subject, string? htmlMessage)
    {
        return new MailMessage()
        {
            From = new MailAddress(emailOptions.Value.Mail!, emailOptions.Value.SenderName),
            Body = string.IsNullOrEmpty(htmlMessage) ? string.Empty : htmlMessage,
            Subject = subject,
            IsBodyHtml = !string.IsNullOrEmpty(htmlMessage)
        };
    }
    private SmtpClient GetSmtpClient()
    {
        return new SmtpClient(emailOptions.Value.Host)
        {
            Port = emailOptions.Value.Port,
            Credentials = new NetworkCredential(emailOptions.Value.Mail, emailOptions.Value.Password),
            EnableSsl = true
        };
    }
}
