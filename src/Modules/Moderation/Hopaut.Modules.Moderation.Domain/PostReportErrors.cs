using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Domain;

/// <summary>Expected failures of the PostReport aggregate; the codes are stable (clean-architecture-ddd skill §5).</summary>
public static class PostReportErrors
{
    public static Error NotFound(PostReportId reportId) =>
        Error.NotFound("moderation.post_report.not_found", $"Post report {reportId} was not found.");

    public static Error NotOpen(PostReportId reportId) =>
        Error.Conflict("moderation.post_report.not_open", $"Post report {reportId} is no longer open.");
}
