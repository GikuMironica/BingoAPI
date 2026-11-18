using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>
/// A user's report of another user (rules recovered from the legacy API, decision D-035): any signed-in user reports any user
/// with a reason and an optional message, and reports the same user again only <see cref="ReportAgainAfter"/> after their
/// last open report on them. An administrator resolves an open report once.
/// </summary>
public sealed class UserReport : AggregateRoot<UserReportId>
{
    /// <summary>The legacy cooldown between two reports of one reporter on one user.</summary>
    public static readonly TimeSpan ReportAgainAfter = TimeSpan.FromDays(7);

    private UserReport() { } // EF Core

    public UserId ReporterId { get; private set; }

    public UserId ReportedUserId { get; private set; }

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

    /// <summary>
    /// Reports the user with an ID taken from the user-reports sequence; refused within <see cref="ReportAgainAfter"/> of the
    /// reporter's last open report on the same user.
    /// </summary>
    public static Result<UserReport> Create(
        UserReportId id, ReportedUser user, UserId reporter, ReportReason reason, ReportMessage? message, DateTimeOffset now)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(id, default);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(reason);

        if (user.ReporterLastOpenReportAt is { } last && now - last < ReportAgainAfter)
            return UserReportErrors.AlreadyReported(user.UserId);

        return new UserReport
        {
            Id = id,
            ReporterId = reporter,
            ReportedUserId = user.UserId,
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
            return UserReportErrors.NotOpen(Id);

        Status = ReportStatus.Resolved;
        ResolvedAt = now;
        ResolvedBy = resolver;
        return Result.Success();
    }
}
