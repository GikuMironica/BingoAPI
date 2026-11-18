namespace Hopaut.Modules.Notifications.Application;

/// <summary>
/// Port for sending email notifications.
/// </summary>
public interface IEmailSender
{
    Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default);
}
