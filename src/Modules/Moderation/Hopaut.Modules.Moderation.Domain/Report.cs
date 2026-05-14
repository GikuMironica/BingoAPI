namespace Hopaut.Modules.Moderation.Domain;

public enum ReportStatus
{
    Open = 0,
    Resolved = 1,
    Dismissed = 2
}

public sealed class PostReport
{
    public int Id { get; set; }
    public long Timestamp { get; set; }
    public int Reason { get; set; }
    public string? Message { get; set; }
    public string ReporterId { get; set; } = default!;
    public string ReportedHostId { get; set; } = default!;
    public int PostId { get; set; }
    public ReportStatus Status { get; set; } = ReportStatus.Open;
}

public sealed class UserReport
{
    public int Id { get; set; }
    public long Timestamp { get; set; }
    public int Reason { get; set; }
    public string? Message { get; set; }
    public string ReporterId { get; set; } = default!;
    public string ReportedUserId { get; set; } = default!;
    public ReportStatus Status { get; set; } = ReportStatus.Open;
}
