using Hopaut.BuildingBlocks.Infrastructure.Persistence;
using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Moderation.Domain;
using Hopaut.SharedKernel;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public sealed class ModerationRepository(ModerationModuleDbContext db) : IModerationRepository
{
    public async Task<PostReportId> NextPostReportIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(ModerationModuleDbContext.SchemaName, ModerationModuleDbContext.PostReportIdSequence, ct));

    public async Task<UserReportId> NextUserReportIdAsync(CancellationToken ct = default) =>
        new(await db.Database.NextSequenceValueAsync(ModerationModuleDbContext.SchemaName, ModerationModuleDbContext.UserReportIdSequence, ct));

    public Task<PostReport?> GetPostReportByIdAsync(PostReportId id, CancellationToken ct = default)
        => db.PostReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<PostReport>> GetOpenPostReportsAsync(CancellationToken ct = default)
        => db.PostReports.Where(r => r.Status == ReportStatus.Open).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public void AddPostReport(PostReport report) => db.PostReports.Add(report);

    public Task<UserReport?> GetUserReportByIdAsync(UserReportId id, CancellationToken ct = default)
        => db.UserReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<UserReport>> GetOpenUserReportsAsync(CancellationToken ct = default)
        => db.UserReports.Where(r => r.Status == ReportStatus.Open).OrderByDescending(r => r.CreatedAt).ToListAsync(ct);

    public void AddUserReport(UserReport report) => db.UserReports.Add(report);

    // TODO (plan Step 9.3): bypasses the unit of work; the post-deleted consumer replaces it.
    public async Task ClosePostReportsByPostAsync(PostId postId, CancellationToken ct = default)
        => await db.PostReports.Where(r => r.PostId == postId && r.Status == ReportStatus.Open)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReportStatus.Resolved), ct);
}
