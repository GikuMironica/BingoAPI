using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>Expected failures of the PostReport aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class PostReportErrors
{
    public static Error NotFound(PostReportId reportId) =>
        Error.NotFound("moderation.post_report.not_found", $"Post report {reportId} was not found.");

    public static Error NotOpen(PostReportId reportId) =>
        Error.Conflict("moderation.post_report.not_open", $"Post report {reportId} is no longer open.");

    public static Error PostNotFound(PostId postId) =>
        Error.NotFound("moderation.post_report.post_not_found", $"Post {postId} was not found.");

    public static Error AlreadyReported(PostId postId) =>
        Error.Conflict("moderation.post_report.already_reported", $"Post {postId} already has an open report by this user.");
}
