namespace Raknah.Services;

public interface IEmailSendar
{
    Task SendEmailAsync(string[] emails, string subject, string? htmlMessage, params string[] filePaths);
    Task SendEmailAsync(string email, string subject, string? htmlMessage, params string[] filePaths);
}
