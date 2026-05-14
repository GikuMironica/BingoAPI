namespace Hopaut.Modules.Notifications.Application;

/// <summary>
/// Port for sending push notifications (OneSignal).
/// </summary>
public interface IPushNotificationSender
{
    Task SendAsync(string userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default);
    Task SendBatchAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default);
}
