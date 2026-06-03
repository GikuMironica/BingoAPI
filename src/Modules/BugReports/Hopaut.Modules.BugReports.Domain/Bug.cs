using Hopaut.SharedKernel;

namespace Hopaut.Modules.BugReports.Domain;

public sealed class Bug : AggregateRoot<BugReportId>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public string Message { get; private set; } = default!;
    public UserId ReporterId { get; private set; }
    public List<BugScreenshot> Screenshots { get; private set; } = [];

    private Bug() { } // EF Core

    public static Bug Create(string message, UserId reporterId, List<string>? screenshotUrls = null)
    {
        var bug = new Bug
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Message = message,
            ReporterId = reporterId
        };

        if (screenshotUrls is { Count: > 0 })
        {
            bug.Screenshots = screenshotUrls.Select(url => new BugScreenshot { Url = url }).ToList();
        }

        return bug;
    }
}

public sealed class BugScreenshot
{
    public BugScreenshotId Id { get; set; }
    public string Url { get; set; } = default!;
    public BugReportId BugId { get; set; }
    public Bug? Bug { get; set; }
}
