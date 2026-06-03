using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

public enum ReportStatus
{
    Open = 0,
    Resolved = 1,
    Dismissed = 2
}

public sealed class PostReport : AggregateRoot<PostReportId>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public int Reason { get; private set; }
    public string? Message { get; private set; }
    public UserId ReporterId { get; private set; }
    public UserId ReportedHostId { get; private set; }
    public PostId PostId { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Open;

    private PostReport() { } // EF Core

    public static PostReport Create(int reason, string? message, UserId reporterId, UserId reportedHostId, PostId postId)
    {
        return new PostReport
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Reason = reason,
            Message = message,
            ReporterId = reporterId,
            ReportedHostId = reportedHostId,
            PostId = postId
        };
    }

    public bool Resolve()
    {
        if (Status != ReportStatus.Open) return false;
        Status = ReportStatus.Resolved;
        return true;
    }
}

public sealed class UserReport : AggregateRoot<UserReportId>
{
    public DateTimeOffset CreatedAt { get; private set; }
    public int Reason { get; private set; }
    public string? Message { get; private set; }
    public UserId ReporterId { get; private set; }
    public UserId ReportedUserId { get; private set; }
    public ReportStatus Status { get; private set; } = ReportStatus.Open;

    private UserReport() { } // EF Core

    public static UserReport Create(int reason, string? message, UserId reporterId, UserId reportedUserId)
    {
        return new UserReport
        {
            CreatedAt = DateTimeOffset.UtcNow,
            Reason = reason,
            Message = message,
            ReporterId = reporterId,
            ReportedUserId = reportedUserId
        };
    }
}
