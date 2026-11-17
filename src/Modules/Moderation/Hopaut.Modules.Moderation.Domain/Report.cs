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

    /// <summary>Creates a report with an ID taken from the post-reports sequence.</summary>
    public static PostReport Create(PostReportId id, int reason, string? message, UserId reporterId, UserId reportedHostId, PostId postId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);

        return new PostReport
        {
            Id = id,
            CreatedAt = now,
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

    /// <summary>Creates a report with an ID taken from the user-reports sequence.</summary>
    public static UserReport Create(UserReportId id, int reason, string? message, UserId reporterId, UserId reportedUserId, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);

        return new UserReport
        {
            Id = id,
            CreatedAt = now,
            Reason = reason,
            Message = message,
            ReporterId = reporterId,
            ReportedUserId = reportedUserId
        };
    }
}
