using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>Expected failures of the UserReport aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class UserReportErrors
{
    public static Error NotFound(UserReportId reportId) =>
        Error.NotFound("moderation.user_report.not_found", $"User report {reportId} was not found.");

    public static Error NotOpen(UserReportId reportId) =>
        Error.Conflict("moderation.user_report.not_open", $"User report {reportId} is no longer open.");

    public static Error AlreadyReported(UserId userId) =>
        Error.Conflict(
            "moderation.user_report.already_reported",
            $"User {userId} already has an open report by this user from the last {UserReport.ReportAgainAfter.Days} days.");

    public static readonly Error InvalidReportedUser =
        Error.Validation(
            "moderation.user_report.invalid_reported_user",
            $"The reported user must be a user ID of 1 to {ReportedUser.MaxIdLength} characters.");
}
