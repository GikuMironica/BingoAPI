using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Application;

/// <summary>PostReport and UserReport aggregate store. Changes are saved by the Moderation unit of work, never by the repository.</summary>
public interface IModerationRepository
{
    /// <summary>Next ID from the post-reports sequence, assigned before the report is created.</summary>
    Task<PostReportId> NextPostReportIdAsync(CancellationToken ct = default);
    /// <summary>Next ID from the user-reports sequence, assigned before the report is created.</summary>
    Task<UserReportId> NextUserReportIdAsync(CancellationToken ct = default);
    Task<PostReport?> GetPostReportByIdAsync(PostReportId id, CancellationToken ct = default);
    Task<List<PostReport>> GetOpenPostReportsAsync(CancellationToken ct = default);
    void AddPostReport(PostReport report);
    Task<UserReport?> GetUserReportByIdAsync(UserReportId id, CancellationToken ct = default);
    Task<List<UserReport>> GetOpenUserReportsAsync(CancellationToken ct = default);
    void AddUserReport(UserReport report);
    Task ClosePostReportsByPostAsync(PostId postId, CancellationToken ct = default);
}
