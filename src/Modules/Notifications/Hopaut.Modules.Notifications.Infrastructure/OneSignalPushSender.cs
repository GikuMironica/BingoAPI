using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Hopaut.Modules.Notifications.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Hopaut.Modules.Notifications.Infrastructure;

public sealed class OneSignalOptions
{
    public string AppId { get; set; } = default!;
    public string ApiKey { get; set; } = default!;
}

public sealed class OneSignalPushSender : IPushNotificationSender
{
    private readonly HttpClient _http;
    private readonly OneSignalOptions _options;
    private readonly ILogger<OneSignalPushSender> _logger;

    public OneSignalPushSender(HttpClient http, IOptions<OneSignalOptions> options, ILogger<OneSignalPushSender> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public Task SendAsync(string userId, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default)
        => SendBatchAsync([userId], title, body, data, ct);

    public async Task SendBatchAsync(IEnumerable<string> userIds, string title, string body, Dictionary<string, string>? data = null, CancellationToken ct = default)
    {
        var payload = new OneSignalNotificationRequest
        {
            AppId = _options.AppId,
            IncludeExternalUserIds = userIds.ToList(),
            Headings = new() { ["en"] = title },
            Contents = new() { ["en"] = body },
            Data = data
        };

        try
        {
            _http.DefaultRequestHeaders.Authorization = new("Basic", _options.ApiKey);
            var response = await _http.PostAsJsonAsync("https://onesignal.com/api/v1/notifications", payload, ct);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send OneSignal push to {Count} users", payload.IncludeExternalUserIds.Count);
        }
    }

    private sealed class OneSignalNotificationRequest
    {
        [JsonPropertyName("app_id")] public string AppId { get; set; } = default!;
        [JsonPropertyName("include_external_user_ids")] public List<string> IncludeExternalUserIds { get; set; } = [];
        [JsonPropertyName("headings")] public Dictionary<string, string> Headings { get; set; } = new();
        [JsonPropertyName("contents")] public Dictionary<string, string> Contents { get; set; } = new();
        [JsonPropertyName("data")] public Dictionary<string, string>? Data { get; set; }
    }
}
