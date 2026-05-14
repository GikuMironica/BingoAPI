using Hopaut.Modules.Moderation.Application;
using Hopaut.Modules.Moderation.Domain;
using Microsoft.EntityFrameworkCore;

namespace Hopaut.Modules.Moderation.Infrastructure;

public sealed class ModerationRepository : IModerationRepository
{
    private readonly ModerationModuleDbContext _db;

    public ModerationRepository(ModerationModuleDbContext db) => _db = db;

    public Task<PostReport?> GetPostReportByIdAsync(int id, CancellationToken ct = default)
        => _db.PostReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<PostReport>> GetOpenPostReportsAsync(CancellationToken ct = default)
        => _db.PostReports.Where(r => r.Status == ReportStatus.Open).OrderByDescending(r => r.Timestamp).ToListAsync(ct);

    public async Task AddPostReportAsync(PostReport report, CancellationToken ct = default)
        => await _db.PostReports.AddAsync(report, ct);

    public Task<UserReport?> GetUserReportByIdAsync(int id, CancellationToken ct = default)
        => _db.UserReports.FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<List<UserReport>> GetOpenUserReportsAsync(CancellationToken ct = default)
        => _db.UserReports.Where(r => r.Status == ReportStatus.Open).OrderByDescending(r => r.Timestamp).ToListAsync(ct);

    public async Task AddUserReportAsync(UserReport report, CancellationToken ct = default)
        => await _db.UserReports.AddAsync(report, ct);

    public async Task ClosePostReportsByPostAsync(int postId, CancellationToken ct = default)
        => await _db.PostReports.Where(r => r.PostId == postId && r.Status == ReportStatus.Open)
            .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, ReportStatus.Resolved), ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}
