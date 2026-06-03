using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;

namespace Hopaut.Modules.Moderation.Application;

public interface IModerationRepository
{
    Task<PostReport?> GetPostReportByIdAsync(PostReportId id, CancellationToken ct = default);
    Task<List<PostReport>> GetOpenPostReportsAsync(CancellationToken ct = default);
    Task AddPostReportAsync(PostReport report, CancellationToken ct = default);

    Task<UserReport?> GetUserReportByIdAsync(UserReportId id, CancellationToken ct = default);
    Task<List<UserReport>> GetOpenUserReportsAsync(CancellationToken ct = default);
    Task AddUserReportAsync(UserReport report, CancellationToken ct = default);

    Task ClosePostReportsByPostAsync(PostId postId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
