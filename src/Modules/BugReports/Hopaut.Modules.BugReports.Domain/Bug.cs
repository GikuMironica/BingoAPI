using Hopaut.SharedKernel;

namespace Hopaut.Modules.BugReports.Domain;

public sealed class Bug : AggregateRoot<BugReportId>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public string Message { get; private set; } = default!;
    public UserId ReporterId { get; private set; }
    public List<BugScreenshot> Screenshots { get; private set; } = [];

    private Bug() { } // EF Core

    /// <summary>Creates a bug report with an ID taken from the bugs sequence.</summary>
    public static Bug Create(BugReportId id, string message, UserId reporterId, List<string>? screenshotUrls, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);

        var bug = new Bug
        {
            Id = id,
            CreatedAt = now,
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
