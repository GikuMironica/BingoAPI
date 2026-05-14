namespace Hopaut.Modules.BugReports.Domain;

public sealed class Bug
{
    public int Id { get; set; }
    public long Timestamp { get; set; }
    public string Message { get; set; } = default!;
    public string ReporterId { get; set; } = default!;
    public List<BugScreenshot> Screenshots { get; set; } = [];
}

public sealed class BugScreenshot
{
    public int Id { get; set; }
    public string Url { get; set; } = default!;
    public int BugId { get; set; }
    public Bug? Bug { get; set; }
}
