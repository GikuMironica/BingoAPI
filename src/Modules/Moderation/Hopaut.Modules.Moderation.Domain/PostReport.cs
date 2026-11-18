using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// A user's report of a post (rules recovered from the legacy API, decision D-035): any signed-in user reports any post with
/// a reason and an optional message, the reported host is the post's host, and a reporter has at most one open report per
/// post. An administrator resolves an open report once.
/// </summary>
public sealed class PostReport : AggregateRoot<PostReportId>
{
    private PostReport() { } // EF Core

    public PostId PostId { get; private set; }

    public UserId ReporterId { get; private set; }

    /// <summary>The host of <see cref="PostId"/> when the report was made.</summary>
    public UserId ReportedHostId { get; private set; }

    public ReportReason Reason { get; private set; }

    public ReportMessage? Message { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public ReportStatus Status { get; private set; }

    /// <summary>When an administrator resolved the report; null while it is open and for reports resolved before plan Step 8.6.</summary>
    public DateTimeOffset? ResolvedAt { get; private set; }

    /// <summary>The administrator who resolved the report; null while it is open and for reports resolved before plan Step 8.6.</summary>
    public UserId? ResolvedBy { get; private set; }

    /// <summary>Optimistic-concurrency token (PostgreSQL <c>xmin</c>): several administrators may resolve the same report.</summary>
    public uint Version { get; private set; }

    /// <summary>Reports the post with an ID taken from the post-reports sequence; refused while the reporter has an open report on it.</summary>
    public static Result<PostReport> Create(
        PostReportId id, ReportedPost post, UserId reporter, ReportReason reason, ReportMessage? message, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(post);
        ArgumentNullException.ThrowIfNull(reason);

        if (post.ReporterHasOpenReport)
            return PostReportErrors.AlreadyReported(post.PostId);

        return new PostReport
        {
            Id = id,
            PostId = post.PostId,
            ReporterId = reporter,
            ReportedHostId = post.HostId,
            Reason = reason,
            Message = message,
            CreatedAt = now,
            Status = ReportStatus.Open,
        };
    }

    /// <summary>An administrator marks the open report as handled; a report is resolved once.</summary>
    public Result Resolve(UserId resolver, DateTimeOffset now)
    {
        if (Status != ReportStatus.Open)
            return PostReportErrors.NotOpen(Id);

        Status = ReportStatus.Resolved;
        ResolvedAt = now;
        ResolvedBy = resolver;
        return Result.Success();
    }
}
