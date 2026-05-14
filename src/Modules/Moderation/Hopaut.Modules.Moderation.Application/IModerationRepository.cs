using Hopaut.Modules.Moderation.Domain;

namespace Hopaut.Modules.Moderation.Application;

public interface IModerationRepository
{
    Task<PostReport?> GetPostReportByIdAsync(int id, CancellationToken ct = default);
    Task<List<PostReport>> GetOpenPostReportsAsync(CancellationToken ct = default);
    Task AddPostReportAsync(PostReport report, CancellationToken ct = default);

    Task<UserReport?> GetUserReportByIdAsync(int id, CancellationToken ct = default);
    Task<List<UserReport>> GetOpenUserReportsAsync(CancellationToken ct = default);
    Task AddUserReportAsync(UserReport report, CancellationToken ct = default);

    Task ClosePostReportsByPostAsync(int postId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
