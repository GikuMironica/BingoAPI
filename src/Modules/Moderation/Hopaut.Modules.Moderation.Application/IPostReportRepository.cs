using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Application;

/// <summary>
/// PostReport aggregate store for the write paths; reads go through <c>IModerationReadModel</c>. Changes are saved by the
/// Moderation unit of work, never by the repository.
/// </summary>
public interface IPostReportRepository
{
    /// <summary>Next ID from the post-reports sequence, assigned before the report is created.</summary>
    Task<PostReportId> NextIdAsync(CancellationToken ct = default);

    Task<PostReport?> GetByIdAsync(PostReportId id, CancellationToken ct = default);

    /// <summary>Whether the reporter has an open report on the post (one open report per reporter and post).</summary>
    Task<bool> HasOpenReportAsync(UserId reporterId, PostId postId, CancellationToken ct = default);

    void Add(PostReport report);
}
